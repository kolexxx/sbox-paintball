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
		public bool Show { get; set; } = false;
		private Dictionary<Client, ScoreboardEntry> _entries = new();
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

			Initialize();

			BindClass( "hidden", () => Local.Hud.GetChild( 10 ).IsVisible );
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "open", Show || Input.Down( InputButton.Score ) );

			if ( !IsVisible )
				return;
		}

		[PBEvent.Player.Killed]
		public void OnPlayerKilled( Player player, Entity attacker )
		{
			for ( int i = 1; i <= 2; i++ )
			{
				_sections[i].SortChildren( e =>
				{
					var client = (e as ScoreboardEntry)?.Client;

					int rank = client.GetInt( "kills" );

					return -rank;
				} );
			}
		}

		public ScoreboardEntry AddEntry( Client client )
		{
			Team team = Team.None;

			if ( client.Pawn.IsValid() )
				team = (client.Pawn as Player).Team;

			var e = _sections[(int)team].AddChild<ScoreboardEntry>();
			e.Client = client;

			return e;
		}

		[PBEvent.Player.Team.Changed]
		public void UpdateEntry( Player player, Team oldTeam )
		{
			var client = player.Client;

			if ( !client.IsValid() )
				return;

			var e = _entries[client];

			if ( _entries.ContainsKey( client ) )
				e?.Delete();

			e = AddEntry( client );
			_entries[client] = e;
		}

		[PBEvent.Client.Joined]
		public void ClientJoined( Client client )
		{
			if ( !client.IsValid() )
			{
				Initialize();

				return;
			}

			if ( _entries.ContainsKey( client ) )
				return;

			var e = AddEntry( client );
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

		[Event.Hotload]
		private void Initialize()
		{
			if ( !Host.IsClient )
				return;

			foreach ( var client in Client.All.Except( _entries.Keys ) )
			{
				var e = AddEntry( client );
				_entries[client] = e;
			}
		}

		public class ScoreboardEntry : Panel
		{
			public Client Client;
			public Label Alive;
			public Label Deaths;
			public Label Kills;
			public Label Name;
			public Label Ping;
			private TimeSince SinceUpdate;

			public ScoreboardEntry()
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

				if ( SinceUpdate < 0.2f )
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
