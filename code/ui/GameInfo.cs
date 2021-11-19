using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace PaintBall
{
	public class GameInfo : Panel
	{
		public Label Timer;
		public Panel left;
		public Middle mid;
		public Panel right;
		public GameInfo()
		{
			StyleSheet.Load( "/ui/GameInfo.scss" );

			left = AddChild<Panel>( "left" );
			left.Add.Label( "0" );

			mid = AddChild<Middle>( "mid" );
			Timer = mid.GetChild( 0 ).Add.Label( "00:00" );

			right = AddChild<Panel>( "right" );
			right.Add.Label( "0" );
		}

		public override void Tick()
		{
			base.Tick();

			var player = Local.Pawn;
			if ( player == null )
				return;

			var game = Game.Instance;
			if ( game == null )
				return;

			var state = game.CurrentGameState;
			if ( state == null )
				return;

			if ( state.UpdateTimer )
				Timer.Text = TimeSpan.FromSeconds( state.TimeLeftSeconds ).ToString( @"mm\:ss" );
			else
				Timer.Text = "00:00";
		}

		public class Middle : Panel
		{
			public Panel Timer;
			public Panel BlueScore;
			public Panel RedScore;

			public Middle()
			{
				Timer = AddChild<Panel>( "timer" );
				BlueScore = AddChild<Panel>( "bluescore" );
				BlueScore.Add.Label( "0" );
				RedScore = AddChild<Panel>( "redscore" );
				RedScore.Add.Label( "0" );
			}
		}
	}

}
