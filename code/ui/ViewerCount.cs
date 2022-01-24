﻿using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Paintball;

public class ViewerCount : Panel
{
	private Image _icon;
	private Label _count;

	public ViewerCount()
	{
		StyleSheet.Load( "/ui/ViewerCount.scss" );

		_icon = Add.Image( "ui/eye.png", "icon" );
		_count = Add.Label( "0", "count" );
	}

	public override void Tick()
	{
		base.Tick();

		var player = (Local.Pawn as Player).CurrentPlayer;

		if ( player == null )
			return;

		SetClass( "hidden", TeamSelect.Instance.IsVisible || player.ViewerCount <= 0 );

		if ( !IsVisible )
			return;		

		_count.Text = player.ViewerCount.ToString();
	}
}

