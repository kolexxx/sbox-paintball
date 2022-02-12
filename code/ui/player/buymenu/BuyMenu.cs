using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Paintball.UI;

// by MoonlyDays

public class BuyMenu : Panel
{
	Panel WheelContainer { get; set; }
	public BuyMenuWheel Wheel { get; set; }
	public static BuyMenu Instance;
	private TimeSince _timeSinceInteraction;

	Label BuyTimeLabel { get; set; }

	public BuyMenu()
	{
		Instance = this;

		StyleSheet.Load( "/ui/player/buymenu/BuyMenu.scss" );

		//
		// WHEEL
		//
		var container = Add.Panel( "container" );
		BuyTimeLabel = container.Add.Label( "Buy Time Remaining: 56:23", "title" );
		WheelContainer = container.Add.Panel( "wheel_container" );


		//
		// FOOTER
		//
		var footer = Add.Panel( "footer" );

		var leftCol = footer.Add.Panel( "left col" );
		var rightCol = footer.Add.Panel( "right col" );

		leftCol.Add.Label( "[C] BACK", "button" ).AddEventListener( "onclick", Hide );

		rightCol.Add.Label( "AUTO BUY", "button" );
		rightCol.Add.Label( "RE-BUY PREVIOUS", "button" );
		rightCol.Add.Label( "BUY FOR TEAMMATE", "button" );

		AddEventListener( "onrightclick", () => Wheel?.HandleRightClick() );
	}

	public override void Tick()
	{
		if ( Local.Pawn is not Player player || !player.Alive() )
			return;

		bool canBuy = player.IsInBuyZone && Game.Current.State.CanBuy;

		if ( _timeSinceInteraction > 0.1f )
		{
			if ( Input.Pressed( InputButton.View ) )
			{
				if ( !IsVisible && !canBuy )
				{
					return;
				}
				_timeSinceInteraction = 0;
				Toggle();
			}


			if ( !IsVisible ) return;

			if ( Game.Current.State is GameplayState state )
				BuyTimeLabel.Text = "Buy Time Remaining: " + TimeSpan.FromSeconds( state.BuyTimeExpire ).ToString( @"mm\:ss" );
			else
				BuyTimeLabel.Text = string.Empty;

			if ( !canBuy ) Hide();
		}

	}

	public void Toggle()
	{
		if ( IsVisible ) Hide();
		else Show();
	}

	public void Hide()
	{
		if ( !HasClass( "visible" ) ) return;

		var player = Local.Pawn as Player;
		bool canBuy = player.IsInBuyZone && Game.Current.State.CanBuy;
		if ( Wheel is BuyMenuWheelGroups || !canBuy )
		{
			SetClass( "visible", false );
			Sound.FromScreen( "buymenu_close" );

			Wheel.Delete( true );
			Wheel = null;
		}
		else if ( Wheel is BuyMenuWheelItems )
		{
			Wheel.Delete( true );
			Wheel = null;

			WheelContainer.AddChild<BuyMenuWheelGroups>();
		}
	}

	public void Show()
	{
		if ( HasClass( "visible" ) ) return;

		SetClass( "visible", true );

		WheelContainer.AddChild<BuyMenuWheelGroups>();
	}
}

public class BuyMenuWheel : Panel
{
	/// <summary>
	/// How many slices does this wheel have?
	/// </summary>
	public int Slices { get; protected set; }
	/// <summary>
	/// How many degrees per slice?
	/// </summary>
	public int Degrees { get; protected set; }
	/// <summary>
	/// Slice panel elements.
	/// </summary>
	List<Panel> SliceElements { get; set; } = new();
	int HoveredSlice { get; set; }
	bool IsInit { get; set; }

	public BuyMenuWheel( int slices )
	{
		AddClass( "wheel" );

		Slices = slices;
		Degrees = 360 / slices;
		SliceElements.Clear();

		var icon = Add.Panel( "icon" );
		icon.BindClass( "blue", () => ((Player)Local.Pawn).Team == Team.Blue );
		icon.BindClass( "red", () => ((Player)Local.Pawn).Team == Team.Red );

		AddEventListener( "onclick", HandleClick );
		Sound.FromScreen( "buymenu_open" );
	}

	/// <summary>
	/// Return the position of the content of a slice, offset from center by distance.
	/// </summary>
	/// <param name="index"></param>
	/// <param name="distance"></param>
	/// <returns></returns>
	public Vector2 GetSliceContentPosition( int index, float distance )
	{
		index = Math.Max( index, 0 );
		index %= Slices;

		float relativeDeg = (index) * Degrees;

		float elOffsetDeg = -(90 + Degrees / 2);
		float elDeg = elOffsetDeg + relativeDeg;
		float elRad = elDeg.DegreeToRadian();

		float sin = MathF.Sin( elRad );
		float cos = MathF.Cos( elRad );

		var offset = new Vector2( cos, sin );
		var center = new Vector2( 300, 300 );

		return center + offset * distance;
	}

	/// <summary>
	/// Create slices.
	/// </summary>
	public void CreateSlices()
	{
		for ( int i = 0; i < Slices; i++ )
		{
			CreateSlice( i );
		}
	}

	/// <summary>
	/// Create a specific slice.
	/// </summary>
	/// <param name="index"></param>
	public virtual void CreateSlice( int index )
	{
		float sliceOffsetDeg = -(180 - Degrees) - Degrees;
		float relativeDeg = index * Degrees;

		float sliceDeg = sliceOffsetDeg + relativeDeg;

		var p = Add.Panel( "wedge" );
		p.Style.Set( "transform", $"rotate({sliceDeg}deg)" );
		p.AddClass( $"{Degrees}" );
		SliceElements.Add( p );

		if ( IsSliceActive( index ) )
		{
			var pos = GetSliceContentPosition( index, 80 );
			var label = Add.Label( $"{index + 1}", "label" );
			label.Style.Set( "left", $"{pos.x}px" );
			label.Style.Set( "top", $"{pos.y}px" );
		}
	}

	public override void Tick()
	{
		base.Tick();

		if ( !IsInit )
		{
			var panel = Parent.Parent.Parent;

			if ( panel is BuyMenu menu )
			{
				var wheel = menu.Wheel;
				if ( wheel != null ) wheel.Delete( true );
				menu.Wheel = this;
			}

			CreateSlices();
			IsInit = true;
		}

		var center = Box.RectOuter.Center;
		var mouse = Mouse.Position;

		float deg = -MathF.Atan2( mouse.x - center.x, mouse.y - center.y ).RadianToDegree() + 180 + Degrees;
		var index = (int)Math.Floor( deg / Degrees ) % Slices;

		OnSliceHovered( index );

		if ( Input.Pressed( InputButton.Attack1 ) ) OnSliceClicked( HoveredSlice );
	}

	public void OnSliceHovered( int index )
	{
		for ( int i = 0; i < SliceElements.Count; i++ )
		{
			var wedge = SliceElements[i];
			if ( wedge != null )
			{
				var shouldHover = i == index;

				wedge.SetClass( "hover", shouldHover && IsSliceActive( i ) );
				if ( shouldHover ) HoveredSlice = i;
			}
		}
	}

	public virtual void OnSliceClicked( int index )
	{
	}

	public virtual bool IsSliceActive( int index )
	{
		return true;
	}

	public virtual void HandleClick()
	{
		OnSliceClicked( HoveredSlice );
	}

	public virtual void HandleRightClick()
	{

	}
}

public class BuyMenuWheelGroups : BuyMenuWheel
{
	public struct GroupDefinition
	{
		public string Name;
		public int Slices;
		public string[] Weapons;

		public GroupDefinition( string name, int slices, string[] weapons )
		{
			Name = name;
			Slices = slices;
			Weapons = weapons;
		}
	}

	static GroupDefinition[] Groups => new GroupDefinition[]
	{
			new GroupDefinition("Pistol", 4, new string[]{ "pb_pistol", string.Empty, string.Empty, string.Empty, string.Empty }),
			new GroupDefinition("Heavy", 4, new string[]{ "pb_shotgun", "pb_autoshotgun", string.Empty, string.Empty, string.Empty }),
			new GroupDefinition("SMG", 4, new string[]{ "pb_smg", "pb_havoc", string.Empty, string.Empty, string.Empty }),
			new GroupDefinition("Rifle", 4, new string[]{ string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty }),
			new GroupDefinition("Equipment", 4, new string[]{ string.Empty, string.Empty, string.Empty, string.Empty }),
			new GroupDefinition("Grenade", 4, new string[]{ "pb_spike", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty }),
	};

	public BuyMenuWheelGroups() : base( Groups.Length ) { }

	public override void CreateSlice( int index )
	{
		base.CreateSlice( index );

		var pos = GetSliceContentPosition( index, 180 );
		var item = Add.Label( Groups[index].Name, "groupname" );
		item.Style.Set( "left", $"{pos.x}px" );
		item.Style.Set( "top", $"{pos.y}px" );
	}

	public override void OnSliceClicked( int index )
	{
		base.OnSliceClicked( index );

		new BuyMenuWheelItems( Groups[index] )
		{
			Parent = Parent
		};

		Delete();
	}
}

public class BuyMenuWheelItems : BuyMenuWheel
{
	BuyMenuWheelGroups.GroupDefinition Group { get; set; }
	Dictionary<int, bool> CanBuy { get; set; } = new();

	public BuyMenuWheelItems( BuyMenuWheelGroups.GroupDefinition group ) : base( group.Slices )
	{
		Group = group;
	}

	public override void HandleRightClick()
	{
		Parent.AddChild<BuyMenuWheelGroups>();
	}

	public override void CreateSlice( int index )
	{
		base.CreateSlice( index );

		CanBuy[index] = false;

		if ( Local.Pawn is Player player )
		{
			if ( index >= Group.Weapons.Count() ) return;

			var name = Group.Weapons[index];

			if ( !ItemConfig.All.TryGetValue( name, out var config ) )
				return;

			var pos = GetSliceContentPosition( index, 180 );

			var item = Add.Panel( "item" );
			item.Style.Set( "left", $"{pos.x}px" );
			item.Style.Set( "top", $"{pos.y}px" );

			item.Add.Label( config.Title, "itemname" );
			item.Add.Image( config.Icon, "itemicon" );
			var price = item.Add.Label( $"${string.Format( "{0:n0}", config.Price )}", "itemprice" );

			CanBuy[index] = player.Money >= config.Price;
			price.SetClass( "locked", !CanBuy[index] );
		}
	}

	public override void OnSliceClicked( int index )
	{
		if ( !IsSliceActive( index ) ) return;

		if ( CanBuy[index] )
		{
			Player.RequestItem( Group.Weapons[index] );

			Parent.AddChild<BuyMenuWheelGroups>();
			Sound.FromScreen( "buymenu_purchase" );
		}
		else
		{
			Sound.FromScreen( "buymenu_purchase_fail" );
		}

	}

	public override bool IsSliceActive( int index )
	{
		return index < Group.Weapons.Count();
	}
}

