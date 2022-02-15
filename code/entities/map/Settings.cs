using Sandbox;

namespace Paintball;

[Hammer.EntityTool( "Game and Map settings", "Paintball" )]
[Library( "pb_settings", Title = "Settings", Spawnable = true )]
public partial class Settings : Entity
{
	[Net, Property] public string BlueTeamName { get; set; } = "Blue";
	[Net, Property] public string RedTeamName { get; set; } = "Red";
	[Property] public int FreezeDuration { get; set; } = 5;
	[Property] public int PlayDuration { get; set; } = 60;
	[Property] public int EndDuration { get; set; } = 5;
	[Property] public int BombDuration { get; set; } = 30;
	[Property] public int RoundLimit { get; set; } = 12;
	protected Output OnRoundStart { get; set; }
	protected Output<Team> OnRoundEnd { get; set; }
	protected Output OnRoundNew { get; set; }
	protected Output OnBlueWin { get; set; }
	protected Output OnRedWin { get; set; }

	public override void Spawn()
	{
		if ( Game.Current.Settings.IsValid() )
		{
			Log.Warning( "You have several Settings entities. This is not allowed!" );
			Delete();

			return;
		}

		base.Spawn();

		Game.Current.Settings = this;
		Parent = Game.Current;
		Transmit = TransmitType.Always;

		Event.Run( PBEvent.Game.SettingsLoaded );
	}

	public override void ClientSpawn()
	{
		if ( IsClientOnly )
		{
			Log.Warning( "Can't create Settings entity on the client!" );
			Delete();
			return;
		}

		base.ClientSpawn();

		Game.Current.Settings = this;
		Event.Run( PBEvent.Game.SettingsLoaded );
	}

	[PBEvent.Round.Start]
	private void RoundStart()
	{
		OnRoundStart.Fire( this );
	}

	[PBEvent.Round.End]
	private void RoundEnd( Team winner )
	{
		OnRoundEnd.Fire( this, winner );

		if ( winner == Team.Blue )
			OnBlueWin.Fire( this );
		else if ( winner == Team.Red )
			OnRedWin.Fire( this );
	}

	[PBEvent.Round.New]
	private void NewRound()
	{
		OnRoundNew.Fire( this );
	}
}
