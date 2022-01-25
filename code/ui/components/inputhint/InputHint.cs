using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Paintball.UI;

public class InputHint : Panel
{
	public Label Context { get; set; }
	public Image Glyph { get; set; }
	private InputButton _button;

	public InputHint()
	{
		Context = Add.Label( "", "context" );
		Glyph = Add.Image( "", "glyph" );
	}

	public void SetButton( InputButton button )
	{
		_button = button;

		var texture = Input.GetGlyph( _button, InputGlyphSize.Small, GlyphStyle.Light.WithSolidABXY() );
		Glyph.Texture = texture;
		Glyph.Style.Width = texture.Width / 1.5f;
		Glyph.Style.Height = texture.Height / 1.5f;
	}
}
