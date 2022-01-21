using Sandbox;

namespace PaintBall;

public partial class CustomWalkController : WalkController
{
	[Net] public new float AirControl { get; set; } = 30.0f;
	[Net] public new float SprintSpeed { get; set; } = 150.0f;
	[Net] public new float WalkSpeed { get; set; } = 150.0f;
	[Net] public new float DefaultSpeed { get; set; } = 250.0f;

	public new Player Pawn
	{
		get => base.Pawn as Player;
		set=> base.Pawn = value;
	}

	public override float GetWishSpeed()
	{
		if ( !Game.Current.State.FreezeTime || Pawn.IsPlantingBomb || Pawn.IsDefusingBomb )
			return 0f;

		var ws = Duck.GetWishSpeed();
		if ( ws >= 0 ) return ws;

		if ( Input.Down( InputButton.Run ) ) return SprintSpeed;
		if ( Input.Down( InputButton.Walk ) ) return WalkSpeed;

		return DefaultSpeed;
	}

	public override void CheckJumpButton()
	{
		if ( !Game.Current.State.FreezeTime || Pawn.IsPlantingBomb || Pawn.IsDefusingBomb )
			return;

		base.CheckJumpButton();
	}
}
