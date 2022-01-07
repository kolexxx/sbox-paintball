using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PaintBall
{
	public class KillConfirmed : Panel
	{
		private Entry _currentEntry;

		public KillConfirmed()
		{
			StyleSheet.Load( "/ui/popup/KillConfirmed.scss" );
		}

		[PBEvent.Player.Killed]
		public void OnPlayerKilled( Player player, Entity attacker )
		{
			if ( attacker != Local.Pawn )
				return;

			_currentEntry?.Delete( true );

			AddChild( new Entry( 5f ) );

			_currentEntry = GetChild( 0 ) as Entry;
			_currentEntry.Name.Text = _currentEntry.Name.Text = $"YOU KILLED {player.Client.Name.ToUpper()}"; ;
			_currentEntry.Icon.SetTexture( $"avatar:{player.Client.PlayerId}" );
			_currentEntry.SetHit( (HitboxGroup)player.GetHitboxGroup( player.LastHitboxIndex ) );
		}

		[PBEvent.Round.New]
		private void RoundStart()
		{
			_currentEntry?.Delete();
		}

		public sealed class Entry : PopUp
		{
			public Image Icon { get; internal set; }
			public Label Name { get; internal set; }
			private Image _head;
			private Image _chest;
			private Image _stomach;
			private Image _leftArm;
			private Image _rightArm;
			private Image _leftLeg;
			private Image _rightLeg;

			public Entry( float lifeTime ) : base( lifeTime )
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
		}
	}
}
