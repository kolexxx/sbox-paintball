using Sandbox;

namespace Paintball;

[Hammer.EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
[Library( "pb_havoc", Title = "Havoc", Spawnable = false )]
public partial class Havoc : ProjectileWeapon<BouncyProjectile>
{
}
