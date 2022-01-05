using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Threading.Tasks;

namespace PaintBall
{
	public class KillConfirmed : Panel
	{
		private KillConfirmedEntry _currentPopUp;

		public KillConfirmed()
		{
			StyleSheet.Load( "/ui/popup/KillConfirmed.scss" );
		}


		[PBEvent.Player.Killed]
		public void OnPlayerKilled( Player player, Entity attacker )
		{
			if ( attacker != Local.Pawn )
				return;

			_currentPopUp?.Delete();
			_currentPopUp = AddChild<KillConfirmedEntry>();
			_currentPopUp.Name.Text = $"YOU KILLED {player.Client.Name.ToUpper()}";
			_currentPopUp.Icon.SetTexture( $"avatar:{player.Client.PlayerId}" );
			_currentPopUp.SetHit( (HitboxGroup)player.GetHitboxGroup( player.LastHitboxIndex ) );
		}

		[PBEvent.Round.New]
		private void RoundStart()
		{
			_currentPopUp?.Delete();
		}

		public class KillConfirmedEntry : Panel
		{
			public Image Icon { get; set; }
			public Label Name { get; set; }
			private Image _head;
			private Image _chest;
			private Image _stomach;
			private Image _leftArm;
			private Image _rightArm;
			private Image _leftLeg;
			private Image _rightLeg;

			public KillConfirmedEntry()
			{
				Icon = Add.Image( "", "icon" );
				Name = Add.Label( "", "name" );
				_head = Add.Image( "ui/terry/head.png", "terry" );
				_chest = Add.Image( "ui/terry/stomach.png", "terry" );
				_stomach = _chest;
				_leftArm = Add.Image( "ui/terry/leftarm.png", "terry" );
				_rightArm = Add.Image( "ui/terry/rightarm.png", "terry" );
				_leftLeg = Add.Image( "ui/terry/leftleg.png", "terry" );
				_rightLeg = Add.Image( "ui/terry/rightleg.png", "terry" );

				_ = DeleteAsync();
			}

			public void SetHit( HitboxGroup hitboxGroup )
			{
				Log.Info( hitboxGroup );
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

			private async Task DeleteAsync()
			{
				await Task.Delay( 5000 );

				Delete();
			}
		}
	}
}
