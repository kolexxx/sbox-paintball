using System;

namespace Paintball;

[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
public class BuyableAttribute : Attribute
{
	public int Price = 0;

	public BuyableAttribute() : base () { }
}
