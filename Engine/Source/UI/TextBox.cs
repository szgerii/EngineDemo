using Engine.UI.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.UI;

public struct Padding {
	public int Left { get; set; }
	public int Top { get; set; }
	public int Right { get; set; }
	public int Bottom { get; set; }

	public Vector2 TopLeftOffset => new(Left, Top);
	public Vector2 Size => new(Left + Right, Top + Bottom);

	public Padding(int left, int top, int right, int bottom) {
		Left = left;
		Right = right;
		Top = top;
		Bottom = bottom;
	}
}

public class TextBox : UIObject {
	public TextElement TextElement { get; set; }
	public BoxElement BoxElement { get; set; }
	public Color ForegroundColor { get; set; } = Color.White;
	public Padding Padding { get; set; }

	public TextBox(Vector2 pos, TextElement text, BoxElement box = null) : base(pos) {
		TextElement = text;
		BoxElement = box;
	}

	public override void Update(GameTime gameTime) {
		if (BoxElement != null && TextElement.Output != null) {
			BoxElement.Size = new Vector2(TextElement.Output.Width, TextElement.Output.Height) + Padding.Size;
		}

		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime) {
		// draw text
		Game.SpriteBatch.Draw(
			TextElement.Output,
			Position + new Vector2(Padding.Left, Padding.Top),
			new Rectangle(0, 0, TextElement.Output.Width, TextElement.Output.Height),
			ForegroundColor,
			0,
			Vector2.Zero,
			1,
			SpriteEffects.None,
			LayerDepth
		);

		if (BoxElement == null) {
			return;
		}

		// draw box
		Game.SpriteBatch.Draw(
			BoxElement.Output,
			Position,
			new Rectangle(0, 0, BoxElement.Output.Width, BoxElement.Output.Height),
			Color.White,
			0,
			Vector2.Zero,
			1,
			SpriteEffects.None,
			LayerDepth + 0.1f
		);
	}
}
