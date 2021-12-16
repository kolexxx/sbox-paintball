using Sandbox;

namespace PaintBall
{
	public partial class CustomWalkController : WalkController
	{
		[Net] public new float AirControl { get; set; } = 30.0f;
		[Net] public new float SprintSpeed { get; set; } = 150.0f;
		[Net] public new float WalkSpeed { get; set; } = 150.0f;
		[Net] public new float DefaultSpeed { get; set; } = 250.0f;
		public override float GetWishSpeed()
		{
			if ( Game.Instance.CurrentGameState.FreezeTime <= 5f )
				return 0f;

			var ws = Duck.GetWishSpeed();
			if ( ws >= 0 ) return ws;


			if ( Input.Down( InputButton.Run ) ) return SprintSpeed;
			if ( Input.Down( InputButton.Walk ) ) return WalkSpeed;

			return DefaultSpeed;
		}

		public override void CheckJumpButton()
		{
			if ( Game.Instance.CurrentGameState.FreezeTime <= 5f )
				return;

			base.CheckJumpButton();
		}
	}
}
