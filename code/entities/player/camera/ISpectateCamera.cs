﻿namespace Paintball;

public interface ISpectateCamera
{
	public void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer );
}
