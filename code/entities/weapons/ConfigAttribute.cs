using Sandbox;
using System;
using System.Collections.Generic;

namespace Paintball;

[AttributeUsage( AttributeTargets.Class, Inherited = false )]
public class ConfigAttribute : Attribute
{
	public SlotType Slot;
	public Team ExclusiveFor;

	public ConfigAttribute( SlotType slot = 0, Team exclusiveFor = 0 ) : base()
	{
		Slot = slot;
		ExclusiveFor = exclusiveFor;
	}
}

public class ItemConfig
{
	// Heavily accessed.
	public readonly Team ExclusiveFor;
	public readonly string Icon;
	public readonly string Name;
	public readonly int Price;
	public readonly SlotType Slot;
	public readonly string Title;
	public static Dictionary<string, ItemConfig> All = new();

	public ItemConfig( Type type )
	{
		foreach ( var attribute in type.GetCustomAttributes( false ) )
		{
			if ( attribute is BuyableAttribute buyableAttribute )
			{
				Price = buyableAttribute.Price;
			}
			else if ( attribute is LibraryAttribute libraryAttribute )
			{
				Icon = libraryAttribute.Icon;
				Name = libraryAttribute.Name;
				Title = libraryAttribute.Title;
			}
			else if ( attribute is ConfigAttribute itemAttribute )
			{
				ExclusiveFor = itemAttribute.ExclusiveFor;
				Slot = itemAttribute.Slot;
			}
		}

		All.Add( Name, this );
	}
}

public enum SlotType : byte
{
	Primary = 0,
	Secondary = 1,
	Melee = 2,
	Utility = 3,
	Deployable = 4,
}
