using Sandbox;
using System.Linq;

namespace Paintball;

public partial class Player
{
	[Net, Change] public int ViewerCount { get; set; }
	public bool IsSpectator => Camera is ISpectateCamera;
	public bool IsSpectatingPlayer => _spectatedPlayer.IsValid();
	public Player CurrentPlayer
	{
		get => _spectatedPlayer ?? this;
		set
		{
			var oldSpectatedPlayer = _spectatedPlayer;
			_spectatedPlayer = value == this ? null : value;

			if ( IsClient && oldSpectatedPlayer != _spectatedPlayer )
			{
				Event.Run( PBEvent.Player.Spectating.Changed, oldSpectatedPlayer, _spectatedPlayer );
				AdjustViewerCount( _spectatedPlayer?.NetworkIdent ?? 0 );
			}
		}
	}
	private int _index = 0;
	private Player _spectatedPlayer;
	private RealTimeSince _timeSincePlayerChanged;

	[ServerCmd]
	public static void AdjustViewerCount( int networkIdent )
	{
		var player = ConsoleSystem.Caller.Pawn as Player;

		if ( !player.IsValid() )
			return;

		if ( player._spectatedPlayer.IsValid() )
		{
			player._spectatedPlayer.ViewerCount--;
			player._spectatedPlayer = null;
		}

		if ( networkIdent == 0 )
			return;

		player._spectatedPlayer = Entity.FindByIndex( networkIdent ) as Player;

		if ( player._spectatedPlayer.IsValid() )
			player._spectatedPlayer.ViewerCount++;
	}

	public void TickPlayerChangeSpectateCamera()
	{
		if ( !IsServer || this.Alive() || !Input.Pressed( InputButton.Jump ) )
			return;

		Camera = Camera switch
		{
			FreeSpectateCamera => new FirstPersonSpectateCamera(),
			FirstPersonSpectateCamera => new ThirdPersonSpectateCamera(),
			ThirdPersonSpectateCamera => new FreeSpectateCamera(),
			FixedSpectateCamera => new FreeSpectateCamera(),
			_ => Camera
		};
	}

	public void MakeSpectator()
	{
		Host.AssertServer();

		Inventory.DeleteContents();
		Controller = null;
		EnableAllCollisions = false;
		EnableDrawing = false;
		LifeState = LifeState.Dead;
		Health = 0;
		Camera = new FreeSpectateCamera();
	}

	public void UpdateSpectatingPlayer( int i )
	{
		if ( _timeSincePlayerChanged < 0.1f )
			return;

		_timeSincePlayerChanged = 0;

		var oldPlayer = CurrentPlayer;

		var validPlayers =
			All.OfType<Player>()
			.Where( x => x.IsValid() && x.Alive() && x != Local.Pawn )
			.ToList();

		if ( validPlayers.Count > 0 )
		{
			_index += i;

			if ( _index >= validPlayers.Count )
				_index = 0;

			if ( _index < 0 )
				_index = validPlayers.Count - 1;

			CurrentPlayer = validPlayers[_index];
		}
		else
		{
			CurrentPlayer = null;
		}

		if ( Camera is ISpectateCamera camera )
			camera.OnSpectatedPlayerChanged( oldPlayer, CurrentPlayer );
	}

	[PBEvent.Player.Killed]
	private void OnPlayerKilled( Player player )
	{
		if ( !IsClient || !player.IsValid() )
			return;

		if ( IsSpectatingPlayer && player == CurrentPlayer )
			UpdateSpectatingPlayer( 1 );
	}

	private void OnViewerCountChanged(int oldValue, int newValue)
	{
		if ( this != (Local.Pawn as Player).CurrentPlayer )
			return;

		UI.ViewerCount.Instance.Update( newValue );
	}
}
