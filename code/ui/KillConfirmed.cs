using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Paintball.UI;

public class KillConfirmed : Popup
{
	public Label Distance { get; init; }
	public Image Icon { get; init; }
	public Label Name { get; init; }
	private static KillConfirmed s_current;
	private Image _head;
	private Image _chest;
	private Image _stomach;
	private Image _leftArm;
	private Image _rightArm;
	private Image _leftLeg;
	private Image _rightLeg;

	public KillConfirmed( float lifeTime ) : base( lifeTime )
	{
		s_current = this;

		StyleSheet.Load( "/ui/KillConfirmed.scss" );

		Distance = Add.Label( string.Empty, "distance" );
		Icon = Add.Image( string.Empty, "icon" );
		Name = Add.Label( string.Empty, "name" );
		_head = Add.Image( "ui/terry/head.png", "terry" );
		_chest = Add.Image( "ui/terry/chest.png", "terry" );
		_stomach = Add.Image( "ui/terry/stomach.png", "terry" );
		_leftArm = Add.Image( "ui/terry/leftarm.png", "terry" );
		_rightArm = Add.Image( "ui/terry/rightarm.png", "terry" );
		_leftLeg = Add.Image( "ui/terry/leftleg.png", "terry" );
		_rightLeg = Add.Image( "ui/terry/rightleg.png", "terry" );

		BindClass( "hidden", () => TeamSelect.Instance.IsVisible );
	}

	[PBEvent.Player.Killed]
	public static void OnPlayerKilled( Player player )
	{
		if ( !Host.IsClient || player.LastAttacker != Local.Pawn )
			return;

		s_current?.Delete( true );

		Local.Hud.AddChild( new KillConfirmed( 5f ) );
		s_current = Local.Hud.GetChild( Local.Hud.ChildrenCount - 1 ) as KillConfirmed;

		s_current.Name.Text = s_current.Name.Text = $"You killed {player.Client.Name}";
		s_current.Icon.SetTexture( $"avatar:{player.Client.PlayerId}" );
		s_current.SetHit( (HitboxGroup)player.GetHitboxGroup( player.LastDamageInfo.HitboxIndex ) );

		if ( player.LastDamageInfo.Weapon is not Knife )
		{
			int distance = (Vector3.DistanceBetween( player.LastDamageInfo.Position, player.Position ) / 12f / 3.2808399f).CeilToInt();
			s_current.Distance.Text = $"Distance: {distance}m";
		}

		Sound.FromScreen( "kill_confirmed" );
	}

	private void SetHit( HitboxGroup hitboxGroup )
	{
		Image bodyPart;

		switch ( hitboxGroup )
		{
			case HitboxGroup.Head:
				bodyPart = _head;
				break;
			case HitboxGroup.Chest:
				bodyPart = _chest;
				break;
			case HitboxGroup.Stomach:
				bodyPart = _stomach;
				break;
			case HitboxGroup.LeftArm:
				bodyPart = _leftArm;
				break;
			case HitboxGroup.RightArm:
				bodyPart = _rightArm;
				break;
			case HitboxGroup.LeftLeg:
				bodyPart = _leftLeg;
				break;
			case HitboxGroup.RightLeg:
				bodyPart = _rightLeg;
				break;
			default:
				return;
		}

		bodyPart.SetClass( "hit", true );
		bodyPart.Style.ZIndex = 100;
	}

	[PBEvent.Round.New]
	private void OnNewRound()
	{
		Delete();
	}

	[PBEvent.Player.Spectating.Changed]
	private void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer )
	{
		Delete( true );
	}
}
