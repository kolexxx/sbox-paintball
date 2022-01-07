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

			BindClass( "hidden", () => Local.Hud.GetChild( 10 ).IsVisible );
		}

		public Panel AddEntry( string left, string right, string method, Team teamLeft, Team teamRight, long lsteamid, long rsteamid )
		{
			bool isLocalClient = Local.Client.PlayerId == lsteamid || Local.Client.PlayerId == rsteamid;

			AddChild( new Entry( isLocalClient ? 8f : 5f ) );

			var e = GetChild( ChildrenCount - 1 ) as Entry;

			e.SetClass( "me", isLocalClient );

			e.Left.Text = left;
			e.Left.SetClass( teamLeft.GetString(), true );

			e.Right.Text = right;
			e.Right.SetClass( teamRight.GetString(), true );

			e.Method.SetTexture( method );

			return e;
		}

		[PBEvent.Round.New]
		private void OnNewRound()
		{
			DeleteChildren();
		}

		public sealed class Entry : PopUp
		{
			public Label Left { get; internal set; }
			public Image Method { get; internal set; }
			public Label Right { get; internal set; }

			public Entry( float lifeTime ) : base( lifeTime )
			{
				Left = Add.Label( "", "left" );
				Method = Add.Image( "", "method" );
				Right = Add.Label( "", "right" );
			}
		}
	}
}
