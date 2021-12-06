using Sandbox;
using System.Linq;

namespace PaintBall
{
	public partial class Player
	{
		public bool IsSpectator => Camera is SpectateCamera;
		public bool IsSpectatingPlayer => SpectatedPlayer != null;
		public Player CurrentPlayer
		{
			get => SpectatedPlayer ?? this;
			set
			{
				SpectatedPlayer = value == this ? null : value;
			}
		}
		private Player SpectatedPlayer;
		private int Index = 0;

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
				Index += i;

				if ( Index >= ValidPlayers.Count )
					Index = 0;

				if ( Index < 0 )
					Index = ValidPlayers.Count - 1;

				CurrentPlayer = ValidPlayers[Index];
			}

			if ( Camera is SpectateCamera camera )
				camera.OnSpectatedPlayerChanged( oldPlayer, CurrentPlayer );
		}
	}
}
