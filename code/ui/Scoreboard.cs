using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace PaintBall
{
	public class Scoreboard : Panel
	{
		Dictionary<Client, ScoreBoardEntry> Entries = new();

		public Scoreboard()
		{
			StyleSheet.Load( "/ui/Scoreboard.scss" );
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "open", Input.Down( InputButton.Score ) );

			if ( !IsVisible )
				return;

			foreach ( var client in Client.All.Except( Entries.Keys ) )
			{
				var e = AddEntry( client );
				Entries[client] = e;
			}

			foreach ( var client in Entries.Keys.Except( Client.All ) )
			{
				if ( Entries.TryGetValue( client, out var e ) )
				{
					e?.Delete();
					Entries.Remove( client );
				}
			}

			// Up to 160 comparisons each tick. Maybe add delay for each sort?
			SortChildren( e =>
			{
				var client = (e as ScoreBoardEntry)?.Client;

				int rank = client.GetInt( "kills" ) - client.GetInt( "deaths" );

				return -rank;
			} );
		}

		public ScoreBoardEntry AddEntry( Client client )
		{
			var e = AddChild<ScoreBoardEntry>();
			e.Client = client;

			return e;
		}

		public class ScoreBoardEntry : Panel
		{
			public Client Client;
			public Label Alive;
			public Label Deaths;
			public Label Kills;
			public Label Name;
			public Label Ping;
			private TimeSince SinceUpdate;

			public ScoreBoardEntry()
			{
				Name = Add.Label( "Name", "name" );
				Kills = Add.Label( "0", "kills" );
				Alive = Add.Label( "", "alive" );
				Deaths = Add.Label( "0", "deaths" );
				Ping = Add.Label( "0", "ping" );
			}

			public override void Tick()
			{
				base.Tick();

				if ( !IsVisible )
					return;

				if ( !Client.IsValid() )
					return;

				if ( SinceUpdate < 0.1f )
					return;

				SinceUpdate = 0f;
				Update();
			}

			public void Update()
			{
				Name.Text = Client.Name;
				Kills.Text = Client.GetInt( "kills" ).ToString();
				Alive.Text = Client.Pawn?.LifeState == LifeState.Alive ? "" : "Dead";
				Deaths.Text = Client.GetInt( "deaths" ).ToString();
				Ping.Text = Client.Ping.ToString();
			}
		}
	}

}
