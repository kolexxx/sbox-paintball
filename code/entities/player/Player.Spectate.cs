using Sandbox;
using System.Linq;

namespace PaintBall
{
	public partial class Player
	{
		public bool IsSpectator => Camera is SpectateCamera;
		public bool IsSpectatingPlayer => SpectadedPlayer != null;
		public Player CurrentPlayer
		{
			get => SpectadedPlayer ?? this;
			set
			{
				SpectadedPlayer = value == this ? null : value;
			}
		}
		private Player SpectadedPlayer;
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

		public void UpdateSpectatingPlayer()
		{
			var oldPlayer = CurrentPlayer;

			CurrentPlayer = null;

			var ValidPlayers = All.OfType<Player>().Where( e => e.LifeState == LifeState.Alive ).ToList();

			if ( ValidPlayers.Count > 0 )
			{
				if ( ++Index >= ValidPlayers.Count )
					Index = 0;

				CurrentPlayer = ValidPlayers[Index];
			}

			if ( Camera is SpectateCamera camera )
				camera.OnSpectatedPlayerChanged( oldPlayer, CurrentPlayer );
		}
	}
}
