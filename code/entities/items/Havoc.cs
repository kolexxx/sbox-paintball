﻿using Sandbox;

namespace Paintball;

[Library( "pb_havoc", Title = "Havoc", Icon = "ui/weapons/smg.png", Spawnable = true )]
[Hammer.EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
public partial class Havoc : ProjectileWeapon<BouncyProjectile>
{
	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 2 );
		anim.SetParam( "aimat_weight", 1.0f );
	}
}
