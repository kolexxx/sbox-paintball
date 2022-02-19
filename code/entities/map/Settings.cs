using Sandbox;

namespace Paintball;

[Hammer.EditorSprite( "materials/editor/env_spark.vmat" )]
[Hammer.EntityTool( "Game and Map settings", "Paintball" )]
[Library( "pb_settings", Title = "Settings", Spawnable = true )]
public partial class Settings : Entity
{
	[Net, Property] public string BlueTeamName { get; set; } = "Blue";
	[Net, Property] public string RedTeamName { get; set; } = "Red";
	[Property, ResourceType( "png" )] public string BlueTeamIcon { get; set; }
	[Property, ResourceType( "png" )] public string RedTeamIcon { get; set; }
	[Property] private int FreezeDuration { get; set; } = 5;
	[Property] private int PlayDuration { get; set; } = 60;
	[Property] private int EndDuration { get; set; } = 5;
	[Property] private int BombDuration { get; set; } = 30;
	[Property] private int RoundLimit { get; set; } = 12;
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
		GameplayState.FreezeDuration = FreezeDuration;
		GameplayState.PlayDuration = PlayDuration;
		GameplayState.EndDuration = EndDuration;
		GameplayState.BombDuration = BombDuration;
		GameplayState.RoundLimit = RoundLimit;

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
