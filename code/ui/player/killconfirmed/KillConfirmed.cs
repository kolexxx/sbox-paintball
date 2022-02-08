using Sandbox;
using Sandbox.UI;

namespace Paintball.UI;

[UseTemplate]
public class KillConfirmed : Popup
{
	public Label Distance { get; init; }
	public Image Avatar { get; init; }
	public Label Name { get; init; }
	public Image Head { get; init; }
	public Image Chest { get; init; }
	public Image Stomach { get; init; }
	public Image LeftArm { get; init; }
	public Image RightArm { get; init; }
	public Image LeftLeg { get; init; }
	public Image RightLeg { get; init; }
	private static KillConfirmed s_current;

	public KillConfirmed( Player player, float lifeTime ) : base( lifeTime )
	{
		s_current = this;

		#region terry
		Head.SetTexture( "ui/terry/head.png" );
		Chest.SetTexture( "ui/terry/chest.png" );
		Stomach.SetTexture( "ui/terry/stomach.png" );
		LeftArm.SetTexture( "ui/terry/leftarm.png" );
		RightArm.SetTexture( "ui/terry/rightarm.png" );
		LeftLeg.SetTexture( "ui/terry/leftleg.png" );
		RightLeg.SetTexture( "ui/terry/rightleg.png" );
		#endregion

		Name.Text = s_current.Name.Text = player.Client.Name;
		Avatar.SetTexture( $"avatar:{player.Client.PlayerId}" );

		(GetType()
			.GetProperty( ((HitboxGroup)player.GetHitboxGroup( player.LastDamageInfo.HitboxIndex )).ToString() )
			.GetValue( this, null ) as Image)
			.SetClass( "hit", true );

		if ( player.LastDamageInfo.Weapon is not Knife )
		{
			int distance = (Vector3.DistanceBetween( player.LastDamageInfo.Position, player.Position ) / 12f / 3.2808399f).CeilToInt();
		}

		BindClass( "hidden", () => TeamSelect.Instance.IsVisible );
		SetClass( player.Team.GetTag(), true );
	}

	[PBEvent.Player.Killed]
	public static void OnPlayerKilled( Player player )
	{
		if ( !Host.IsClient || player.LastAttacker != Local.Pawn )
			return;

		s_current?.Delete( true );

		Local.Hud.AddChild( new KillConfirmed( player, 5f ) );

		Sound.FromScreen( "kill_confirmed" );
	}

	[PBEvent.Round.New]
	private void OnNewRound()
	{
		Delete( true );
	}

	[PBEvent.Player.Spectating.Changed]
	private void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer )
	{
		Delete( true );
	}
}
