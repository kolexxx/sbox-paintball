using Sandbox;

namespace Paintball;

[Library( "pb_map_settings", Title = "Map Settings", Spawnable = true )]
public partial class MapSettings : Entity
{
	[Net, Property] public string BlueTeamName { get; set; } = "Blue";
	[Net, Property] public string RedTeamName { get; set; } = "Red";
	protected Output RoundStart { get; set; }
	protected Output RoundEnd { get; set; }
	protected Output RoundNew { get; set; }

	public override void Spawn()
	{
		if ( Game.Current.Map.Settings.IsValid() )
		{
			Log.Warning( "You have several Map Settings. This is not allowed!" );
			Delete();
			return;
		}

		base.Spawn();

		Game.Current.Map.Settings = this;
		Transmit = TransmitType.Always;

		Event.Run( PBEvent.Game.Map.SettingsLoaded );
	}

	public override void ClientSpawn()
	{
		if ( IsClientOnly )
		{
			Log.Warning( "Can't create Map Settings on the client!" );
			Delete();
			return;
		}

		base.ClientSpawn();

		Game.Current.Map.Settings = this;
		Event.Run( PBEvent.Game.Map.SettingsLoaded );
	}

	[PBEvent.Round.Start]
	private void OnRoundStart()
	{
		RoundStart.Fire( this );
	}

	[PBEvent.Round.End]
	private void OnRoundEnd( Team winner )
	{
		RoundEnd.Fire( this );
	}

	[PBEvent.Round.New]
	private void OnNewRound()
	{
		RoundNew.Fire( this );
	}
}
