using Sandbox;

namespace Paintball;

[Hammer.EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
[Library( "pb_smg", Title = "SMG", Spawnable = false )]
public partial class SMG : ProjectileWeapon<BaseProjectile>
{
}
