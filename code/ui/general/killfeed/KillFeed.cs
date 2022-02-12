using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Paintball.UI;

public sealed class KillFeed : Panel
{
	public static KillFeed Instance;

	public KillFeed()
	{
		Instance = this;

		StyleSheet.Load( "/ui/general/killfeed/KillFeed.scss" );

		BindClass( "hidden", () => TeamSelect.Instance.IsVisible );
	}

	[PBEvent.Player.Killed]
	public void OnPlayerKilled( Player player )
	{
		var attacker = player.LastAttacker;

		bool isLocalClient = attacker.Client?.PlayerId == Local.Client.PlayerId;
		isLocalClient = isLocalClient || Local.Client.PlayerId == player.Client.PlayerId;

		Instance.AddChild( new Entry( isLocalClient ? 8f : 5f ) );
		var e = this.LastChild() as Entry;

		e.SetClass( "me", isLocalClient );

		if ( attacker is Player )
		{
			e.Left.Text = attacker.Client?.Name ?? attacker.Name;
			e.Left.SetClass( (attacker as Player)?.Team.GetTag(), true );
		}
		else
		{
			e.Left.Text = player.Client.Name;
			e.Left.SetClass( player.Team.GetTag(), true );
		}

		e.Right.Text = player.Client.Name;
		e.Right.SetClass( player.Team.GetTag(), true );

		if ( player.LastWeaponConfig != null )
			e.Method.SetTexture( player.LastWeaponConfig.Icon );
	}

	[PBEvent.Round.New]
	private void OnNewRound()
	{
		DeleteChildren( true );
	}

	[PBEvent.Game.StateChanged]
	private void OnStatechanged( BaseState oldState, BaseState newState )
	{
		DeleteChildren( true );
	}

	public sealed class Entry : Popup
	{
		public Label Left { get; init; }
		public Image Method { get; init; }
		public Label Right { get; init; }

		public Entry( float lifeTime ) : base( lifeTime )
		{
			Left = Add.Label( string.Empty, "left" );
			Method = Add.Image( string.Empty, "method" );
			Right = Add.Label( string.Empty, "right" );
		}
	}
}
