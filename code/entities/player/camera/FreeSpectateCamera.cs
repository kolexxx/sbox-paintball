using Sandbox;

namespace PaintBall
{
	public class FreeSpectateCamera : Camera, SpectateCamera
	{
		private Angles MoveAngles;
		private Vector3 MoveInput;
		private float MoveSpeed;

		public override void Activated()
		{
			base.Activated();

			Position = CurrentView.Position;
			Rotation = CurrentView.Rotation;

			MoveAngles = Rotation.Angles();
		}

		public override void BuildInput( InputBuilder input )
		{
			MoveInput = input.AnalogMove;

			MoveSpeed = 1f;

			if ( input.Down( InputButton.Run ) )
				MoveSpeed = 5f;

			if ( input.Down( InputButton.Duck ) )
				MoveSpeed = 0.5f;

			MoveAngles += input.AnalogLook;
			MoveAngles.roll = 0;

			base.BuildInput( input );
		}

		public void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer ) { }

		public override void Update()
		{
			if ( Local.Client == null )
				return;

			var Move = MoveInput.Normal * 300 * RealTime.Delta * Rotation * MoveSpeed;

			Position += Move;
			Rotation = Rotation.From( MoveAngles );
		}
	}
}
