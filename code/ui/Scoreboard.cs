using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace PaintBall
{
	public class Scoreboard : Panel
	{

		public static Scoreboard Instance;

		Dictionary<Client, ScoreBoardEntry> Entries = new();

		Panel Header;
		Panel[] Sections = new Panel[3];

		public Scoreboard()
		{
			Instance = this;

			StyleSheet.Load( "/ui/Scoreboard.scss" );

			Sections[0] = Add.Panel( "none" );
			Sections[1] = Add.Panel( "blue" );
			Sections[2] = Add.Panel( "red" );
			Header = Add.Panel( "header" );
			Header.Add.Label( "Name" );
			Header.Add.Label( "" );
			Header.Add.Label( "Kills" );
			Header.Add.Label( "Deaths" );
			Header.Add.Label( "Ping" );
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "open", Input.Down( InputButton.Score ) );

			if ( !IsVisible )
				return;

			foreach ( var client in Client.All.Except( Entries.Keys ) )
			{
				Team team = Team.None;

				if ( client.Pawn != null )
					team = (client.Pawn as Player).Team;

				var e = AddEntry( client, team );
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
			for ( int i = 1; i <= 2; i++ )
			{
				Sections[i].SortChildren( e =>
				{
					var client = (e as ScoreBoardEntry)?.Client;

					int rank = client.GetInt( "kills" ) - client.GetInt( "deaths" );

					return -rank;
				} );
			}
		}

	public ScoreBoardEntry AddEntry( Client client, Team team )
		{
			var e = Sections[(int)team].AddChild<ScoreBoardEntry>();
			e.Client = client;

			return e;
		}

		public void UpdateEntry( Client client, Team team )
		{
			if ( !client.IsValid() )
				return;

			if ( Entries.ContainsKey( client ) )
			{
				var e = Entries[client];
				e?.Delete();

				e = AddEntry( client, team );
				Entries[client] = e;
			}
			else
			{
				var e = AddEntry( client, team );
				Entries[client] = e;
			}
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
				Alive = Add.Label( "", "alive" );
				Kills = Add.Label( "0", "kills" );
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

				if ( !Client.Pawn.IsValid() || Client.Pawn.LifeState != LifeState.Alive )
					Alive.Text = "Dead";
				else
					Alive.Text = "";

				Deaths.Text = Client.GetInt( "deaths" ).ToString();
				Ping.Text =  Client.IsBot ? "BOT" : Client.Ping.ToString();
			}
		}
	}

}
