using Sandbox;
using System.Linq;

namespace PaintBall
{
	public partial class Player
	{
		public bool IsSpectator => Camera is ISpectateCamera;
		public bool IsSpectatingPlayer => _spectatedPlayer != null;
		public Player CurrentPlayer
		{
			get => _spectatedPlayer ?? this;
			set
			{
				_spectatedPlayer = value == this ? null : value;
			}
		}
		private Player _spectatedPlayer;
		private int _index = 0;

		public void ChangeSpectateCamera()
		{
			if ( !IsServer || !Input.Pressed( InputButton.Jump ) )
				return;

			using ( Prediction.Off() )
			{
				Camera = Camera switch
				{
					FreeSpectateCamera => new FirstPersonSpectateCamera(),
					FirstPersonSpectateCamera => new FreeSpectateCamera(),
					_ => Camera
				};
			}
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
			var oldPlayer = CurrentPlayer;

			CurrentPlayer = null;

			var ValidPlayers =
				All.OfType<Player>()
				.Where( x => x.IsValid() && x.LifeState == LifeState.Alive )
				.OrderByDescending( x => x.Name )
				.ToList();

			if ( ValidPlayers.Count > 0 )
			{
				_index += i;

				if ( _index >= ValidPlayers.Count )
					_index = 0;

				if ( _index < 0 )
					_index = ValidPlayers.Count - 1;

				CurrentPlayer = ValidPlayers[_index];
			}

			if ( Camera is ISpectateCamera camera )
				camera.OnSpectatedPlayerChanged( oldPlayer, CurrentPlayer );
		}
	}
}
