using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PaintBall;

public class InputHint : Panel
{
	public Image Glyph { get; set; }
	private InputButton _button;

	public InputHint()
	{
		Glyph = Add.Image( "", "glyph" );
	}

	public void SetButton( InputButton button )
	{
		_button = button;

		var texture = Input.GetGlyph( _button, InputGlyphSize.Small, GlyphStyle.Knockout.WithSolidABXY() );
		Glyph.Texture = texture;
		Glyph.Style.Width = texture.Width / 1.5f;
		Glyph.Style.Height = texture.Height / 1.5f;
	}
}
