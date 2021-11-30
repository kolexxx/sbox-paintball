using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Threading.Tasks;

namespace PaintBall
{
	public class KillFeed : Panel
	{
		public static KillFeed Instance;

		public KillFeed()
		{
			Instance = this;

			StyleSheet.Load( "/ui/KillFeed.scss" );
		}

		public virtual Panel AddEntry( string left, string right, string method, Team teamLeft, Team teamRight, long lsteamid, long rsteamid )
		{
			var e = AddChild<KillFeedEntry>();

			e.SetClass( "me", Local.Client.PlayerId == lsteamid || Local.Client.PlayerId == rsteamid );

			e.Left.Text = left;
			e.Left.SetClass( teamLeft.GetString(), true );

			e.Right.Text = right;
			e.Right.SetClass( teamRight.GetString(), true );

			e.Method.SetTexture( method );

			return e;
		}

		public class KillFeedEntry : Panel
		{
			public Label Left { get; internal set; }
			public Label Right { get; internal set; }
			public Image Method { get; internal set; }

			public KillFeedEntry()
			{
				Left = Add.Label( "", "left" );
				Method = Add.Image( "", "method" );
				Right = Add.Label( "", "right" );
				_ = DeleteAsync();
			}

			private async Task DeleteAsync()
			{
				await Task.Delay( 5000 );
				Delete();
			}
		}
	}
}
