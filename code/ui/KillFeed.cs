using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PaintBall
{
	public partial class KillFeed : Panel
	{
		public static KillFeed Instance;

		public KillFeed()
		{
			Instance = this;

			StyleSheet.Load( "/ui/KillFeed.scss" );

			BindClass( "hidden", () => TeamSelect.Instance.IsVisible );
		}

		public void AddEntry( string left, string right, string method, Team teamLeft, Team teamRight, long lsteamid, long rsteamid )
		{
			Host.AssertClient();

			bool isLocalClient = Local.Client.PlayerId == lsteamid || Local.Client.PlayerId == rsteamid;

			Instance.AddChild( new Entry( isLocalClient ? 8f : 5f ) );

			var e = Instance.GetChild( Instance.ChildrenCount - 1 ) as Entry;

			e.SetClass( "me", isLocalClient );

			e.Left.Text = left;
			e.Left.SetClass( teamLeft.GetString(), true );

			e.Right.Text = right;
			e.Right.SetClass( teamRight.GetString(), true );

			e.Method.SetTexture( method );
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
}
