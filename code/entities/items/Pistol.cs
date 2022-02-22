using Sandbox;

namespace Paintball;

[Hammer.EditorModel( "weapons/rust_pistol/rust_pistol.vmdl" )]
[Library( "pb_pistol", Title = "Pistol", Spawnable = false )]
public partial class Pistol : ProjectileWeapon<BouncyProjectile>
{
}
