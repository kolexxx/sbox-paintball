using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

/// <summary>
/// When a player is within radius of the camera we add this to their entity.
/// We remove it again when they go out of range.
/// </summary>

namespace PaintBall
{
	internal class NameTagComponent : EntityComponent<Player>
	{
		NameTag NameTag;

		protected override void OnActivate()
		{
			NameTag = new NameTag( Entity.Client?.Name ?? Entity.Name, Entity.Client?.PlayerId );
		}

		protected override void OnDeactivate()
		{
			NameTag?.Delete();
			NameTag = null;
		}

		/// <summary>
		/// Called for every tag, while it's active
		/// </summary>
		[Event.Frame]
		public void FrameUpdate()
		{
			var tx = Entity.GetAttachment( "hat" ) ?? Entity.Transform;
			tx.Position += Vector3.Up * 5.0f;
			tx.Rotation = Rotation.LookAt( -CurrentView.Rotation.Forward );

			NameTag.Transform = tx;
		}

		/// <summary>
		/// Called once per frame to manage component creation/deletion
		/// </summary>
		[Event.Frame]
		public static void SystemUpdate()
		{
			var local = Local.Pawn as Player;
			foreach ( var player in Sandbox.Entity.All.OfType<Player>() )
			{
				if ( (player.IsLocalPawn && player.IsFirstPersonMode) || player.LifeState != LifeState.Alive || player == local.CurrentPlayer )
				{
					var c = player.Components.Get<NameTagComponent>();
					c?.Remove();
					continue;
				}

				if ( player.Position.Distance( CurrentView.Position ) > 250 || (local.Team != Team.None && local.Team != player.Team) )
				{
					var c = player.Components.Get<NameTagComponent>();
					c?.Remove();
					continue;
				}

				// Add a component if it doesn't have one
				if ( local.Team == Team.None || local.Team == player.Team )
					player.Components.GetOrCreate<NameTagComponent>();
			}
		}
	}

	/// <summary>
	/// A nametag panel in the world
	/// </summary>
	public class NameTag : WorldPanel
	{
		public Panel Avatar;
		public Label NameLabel;

		internal NameTag( string title, long? steamid )
		{
			StyleSheet.Load( "/ui/NameTag.scss" );

			if ( steamid != null )
			{
				Avatar = Add.Panel( "avatar" );
				Avatar.Style.SetBackgroundImage( $"avatar:{steamid}" );
			}

			NameLabel = Add.Label( title, "title" );

			// this is the actual size and shape of the world panel
			PanelBounds = new Rect( -500, -100, 1000, 200 );
		}
	}
}
