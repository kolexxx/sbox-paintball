using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Paintball;

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

		var e = Instance.GetChild( Instance.ChildrenCount - 1 ) as Entry;

		e.SetClass( "me", isLocalClient );

		if ( attacker is Player )
		{
			e.Left.Text = attacker.Client?.Name ?? attacker.Name;
			e.Left.SetClass( (attacker as Player)?.Team.GetString(), true );
		}
		else
		{
			e.Left.Text = player.Client.Name;
			e.Left.SetClass( player.Team.GetString(), true );
		}

		e.Right.Text = player.Client.Name;
		e.Right.SetClass( player.Team.GetString(), true );

		e.Method.SetTexture( (player.LastDamageInfo.Weapon as Weapon)?.Icon );
	}

	[PBEvent.Round.New]
	private void OnNewRound()
	{
		DeleteChildren();
	}

	[PBEvent.Game.StateChanged]
	private void OnStatechanged( BaseState oldState, BaseState newState )
	{
		DeleteChildren();
	}

	public sealed class Entry : Popup
	{
		public Label Left { get; init; }
		public Image Method { get; init; }
		public Label Right { get; init; }

		public Entry( float lifeTime ) : base( lifeTime )
		{
			Left = Add.Label( "", "left" );
			Method = Add.Image( "", "method" );
			Right = Add.Label( "", "right" );
		}
	}
}
