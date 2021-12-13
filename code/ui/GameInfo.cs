using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace PaintBall
{
	public class GameInfo : Panel
	{
		public static GameInfo Instance;
		public Label Timer;
		public Panel Left;
		public Middle Mid;
		public Panel Right;

		public GameInfo()
		{
			Instance = this;

			StyleSheet.Load( "/ui/GameInfo.scss" );

			Left = AddChild<Panel>( "left" );
			Left.Add.Label( "0" );

			Mid = AddChild<Middle>( "mid" );
			Timer = Mid.GetChild( 0 ).Add.Label( "00:00" );

			Right = AddChild<Panel>( "right" );
			Right.Add.Label( "0" );

			BindClass( "hidden", () => Local.Hud.GetChild( 6 ).IsVisible || Local.Hud.GetChild( 9 ).IsVisible );
		}

		public override void Tick()
		{
			base.Tick();

			if ( !IsVisible )
				return;

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
				Timer.Text = "";
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
