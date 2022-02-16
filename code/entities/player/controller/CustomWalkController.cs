using Sandbox;

namespace Paintball;

public partial class CustomWalkController : WalkController
{
	[Net] public new float AirControl { get; set; } = 30.0f;
	[Net] public new float Gravity { get; set; } = 1200.0f;
	[Net] public new float SprintSpeed { get; set; } = 150.0f;
	[Net] public new float WalkSpeed { get; set; } = 150.0f;
	[Net] public new float DefaultSpeed { get; set; } = 250.0f;

	public new Player Pawn
	{
		get => base.Pawn as Player;
		set => base.Pawn = value;
	}

	public override float GetWishSpeed()
	{
		if ( Pawn.IsFrozen || Pawn.IsPlantingBomb || Pawn.IsDefusingBomb )
			return 0f;

		var ws = Duck.GetWishSpeed();
		if ( ws >= 0 ) return ws;

		if ( Input.Down( InputButton.Run ) ) return SprintSpeed;
		if ( Input.Down( InputButton.Walk ) ) return WalkSpeed;

		float movementSpeedMultiplier = 1f;

		if ( Pawn.ActiveChild is Carriable carriable )
			movementSpeedMultiplier = carriable.Info.MovementSpeedMultiplier;

		return DefaultSpeed * movementSpeedMultiplier;
	}

	public override void CheckJumpButton()
	{
		if ( Pawn.IsFrozen || Pawn.IsPlantingBomb || Pawn.IsDefusingBomb )
			return;

		base.CheckJumpButton();
	}
}
