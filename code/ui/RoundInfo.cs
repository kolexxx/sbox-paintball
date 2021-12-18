using Sandbox;
using Sandbox.UI;
using System;

namespace PaintBall
{
	[UseTemplate]
	public class RoundInfo : Panel
	{
		public static RoundInfo Instance;
		public Label Timer { get; set; }
		public Panel Left { get; set; }
		public Panel Middle { get; set; }
		public Panel Right { get; set; }

		public RoundInfo()
		{
			Instance = this;

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
	}
}
