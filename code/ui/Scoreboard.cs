using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace PaintBall
{
	public class Scoreboard : Panel
	{
		public bool Show { get; set; } = false;
		public static Scoreboard Instance;
		private Dictionary<Client, ScoreBoardEntry> _entries = new();
		private Panel _header;
		private Panel[] _sections = new Panel[3];

		public Scoreboard()
		{
			Instance = this;

			StyleSheet.Load( "/ui/Scoreboard.scss" );

			_sections[0] = Add.Panel( "none" );
			_sections[1] = Add.Panel( "blue" );
			_sections[2] = Add.Panel( "red" );
			_header = Add.Panel( "header" );
			_header.Add.Label( "Name" );
			_header.Add.Label( "" );
			_header.Add.Label( "Kills" );
			_header.Add.Label( "Deaths" );
			_header.Add.Label( "Ping" );

			BindClass( "hidden", () => Local.Hud.GetChild( 9 ).IsVisible );
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "open", Show || Input.Down( InputButton.Score ) );

			if ( !IsVisible )
				return;

			// Up to 160 comparisons each tick. Maybe add delay for each sort?
			for ( int i = 1; i <= 2; i++ )
			{
				_sections[i].SortChildren( e =>
				{
					var client = (e as ScoreBoardEntry)?.Client;

					int rank = client.GetInt( "kills" );

					return -rank;
				} );
			}
		}

		public ScoreBoardEntry AddEntry( Client client, Team team )
		{
			var e = _sections[(int)team].AddChild<ScoreBoardEntry>();
			e.Client = client;

			return e;
		}

		public void UpdateEntry( Client client, Team team )
		{
			if ( !client.IsValid() )
				return;

			if ( _entries.ContainsKey( client ) )
			{
				var e = _entries[client];
				e?.Delete();

				e = AddEntry( client, team );
				_entries[client] = e;
			}
			else
			{
				var e = AddEntry( client, team );
				_entries[client] = e;
			}
		}

		[PBEvent.Client.Joined]
		public void ClientJoined( Client client )
		{
			Team team = Team.None;

			if ( client.Pawn.IsValid() )
				team = (client.Pawn as Player).Team;

			var e = AddEntry( client, team );
			_entries[client] = e;
		}

		[PBEvent.Client.Disconnected]
		public void ClientDisconnected( Client client, NetworkDisconnectionReason reason )
		{
			if ( _entries.TryGetValue( client, out var e ) )
			{
				e?.Delete();
				_entries.Remove( client );
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
				Ping.Text = Client.IsBot ? "BOT" : Client.Ping.ToString();
			}
		}
	}

}
