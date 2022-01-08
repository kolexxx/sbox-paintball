using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PaintBall
{
	public class KillConfirmed : Popup
	{
		public Image Icon { get; init; }
		public Label Name { get; init; }
		private Image _head;
		private Image _chest;
		private Image _stomach;
		private Image _leftArm;
		private Image _rightArm;
		private Image _leftLeg;
		private Image _rightLeg;
		private static KillConfirmed s_current;

		public KillConfirmed( float lifeTime ) : base( lifeTime )
		{
			s_current = this;

			StyleSheet.Load( "/ui/KillConfirmed.scss" );

			Icon = Add.Image( "", "icon" );
			Name = Add.Label( "", "name" );
			_head = Add.Image( "ui/terry/head.png", "terry" );
			_chest = Add.Image( "ui/terry/stomach.png", "terry" );
			_stomach = _chest;
			_leftArm = Add.Image( "ui/terry/leftarm.png", "terry" );
			_rightArm = Add.Image( "ui/terry/rightarm.png", "terry" );
			_leftLeg = Add.Image( "ui/terry/leftleg.png", "terry" );
			_rightLeg = Add.Image( "ui/terry/rightleg.png", "terry" );

			BindClass( "hidden", () => TeamSelect.Instance.IsVisible );
		}

		[PBEvent.Player.Killed]
		public static void OnPlayerKilled( Player player, Entity attacker )
		{
			if ( !Host.IsClient )
				return;

			if ( attacker != Local.Pawn )
				return;

			s_current?.Delete( true );

			Local.Hud.AddChild( new KillConfirmed( 5f ) );

			s_current = Local.Hud.GetChild( Local.Hud.ChildrenCount - 1 ) as KillConfirmed;
			s_current.Name.Text = s_current.Name.Text = $"YOU KILLED {player.Client.Name.ToUpper()}"; ;
			s_current.Icon.SetTexture( $"avatar:{player.Client.PlayerId}" );
			s_current.SetHit( (HitboxGroup)player.GetHitboxGroup( player.LastHitboxIndex ) );
		}

		public void SetHit( HitboxGroup hitboxGroup )
		{
			switch ( hitboxGroup )
			{
				case HitboxGroup.Head:
					_head.SetClass( "hit", true );
					return;
				case HitboxGroup.Chest:
					_chest.SetClass( "hit", true );
					return;
				case HitboxGroup.Stomach:
					_stomach.SetClass( "hit", true );
					return;
				case HitboxGroup.LeftArm:
					_leftArm.SetClass( "hit", true );
					return;
				case HitboxGroup.RightArm:
					_rightArm.SetClass( "hit", true );
					return;
				case HitboxGroup.LeftLeg:
					_leftLeg.SetClass( "hit", true );
					return;
				case HitboxGroup.RightLeg:
					_rightLeg.SetClass( "hit", true );
					return;
			}
		}

		[PBEvent.Round.New]
		private void OnNewRound()
		{
			Delete();
		}
	}
}
