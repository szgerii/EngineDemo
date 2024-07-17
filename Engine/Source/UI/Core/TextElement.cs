using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Engine.UI.Core;

public enum TextAlign {
	Left,
	Right,
	Center,
	Justify
}

public class TextElement : IDisposable {
	public bool NeedsLines { get; private set; } = true;
	public bool NeedsVerts { get; private set; } = true;
	public bool NeedsRedraw { get; set; } = true;
	public bool Dynamic { get; private set; } = true;
	private int currentLimit;
	public VertexSDFText[] Verts { get; set; }
	public int VertexCount { get; set; }
	public RenderTarget2D Output { get; private set; }

	public TextElement(string text, string font, float fontSize, float width, AdvancedColor foreground, bool dynamic = true, TextAlign textAlign = TextAlign.Left, int glyphLimit = -1) {
		this.text = text;
		this.font = font;
		this.fontSize = fontSize;
		this.width = width;
		this.foreground = foreground;
		this.textAlign = textAlign;
		this.glyphLimit = glyphLimit;
		this.Dynamic = dynamic;
		if (!Dynamic) {
			currentLimit = text.Length;
			AllocArray();
		} else {
			UpdateLimit();
			AllocArray();
		}
		if (font != "") {
			TextRenderer.Fonts[font].AddElement(this);
		}
	}


	private List<TextLine> lines = new List<TextLine>();
	/// <summary>
	/// Information associated with each line of text in this text element
	/// </summary>
	public IReadOnlyList<TextLine> Lines {
		get {
			if (NeedsLines) {
				UpdateLines();
			}
			return lines;
		}
	}

	private float width;
	/// <summary>
	/// The width of the 'bounding box' of this text element
	/// lines will word wrapped (if possible), so that text
	/// will fit this box. (Also controls the width of the output texture)
	/// </summary>
	public float Width {
		get => width;
		set {
			width = value;
			NeedsLines = true;
			NeedsVerts = true;
			NeedsRedraw = true;
		}
	}

	private float cachedHeight = -1;
	/// <summary>
	/// The total height of this text element
	/// (also used as the height of the output texture)
	/// </summary>
	public float Height {
		get {
			if (NeedsLines) {
				UpdateLines();
			}
			return cachedHeight;
		}
	}

	private string font = "";
	/// <summary>
	/// The font used to draw this text element
	/// Changing this property will also automatically
	/// assign the element to the specified font
	/// </summary>
	public string Font {
		get => font;
		set {
			if (font != value) {
				if (font != "") {
					TextRenderer.Fonts[font].RemoveElement(this);
				}
				if (value != "") {
					TextRenderer.Fonts[value].AddElement(this);
					font = value;
					NeedsLines = true;
					NeedsVerts = true;
					NeedsRedraw = true;
				}
			}
		}
	}

	private string text = "";
	/// <summary>
	/// The text that is drawn
	/// CAN ONLY BE SET ON A DYNAMIC TEXT ELEMENT!!!
	/// </summary>
	public string Text {
		get => text;
		set {
			if (!Dynamic) {
				throw new Exception("Trying to modify text of a non-dynamic text element.");
			}
			text = value;
			NeedsLines = true;
			NeedsVerts = true;
			NeedsRedraw = true;
			if (UpdateLimit()) {
				AllocArray();
			}
		}
	}

	private float fontSize;
	/// <summary>
	/// The font size (height of a standard glyph, in pixels)
	/// of the drawn text
	/// </summary>
	public float FontSize {
		get => fontSize;
		set {
			fontSize = value;
			NeedsLines = true;
			NeedsVerts = true;
			NeedsRedraw = true;
		}
	}

	private TextAlign textAlign = TextAlign.Left;
	/// <summary>
	/// The horizontal alignment of the text
	/// </summary>
	public TextAlign TextAlign {
		get => textAlign;
		set {
			textAlign = value;
			NeedsVerts = true;
			NeedsRedraw = true;
		}
	}

	private AdvancedColor foreground = Color.White;
	/// <summary>
	/// An xna color or gradient used to draw the glyphs
	/// (if frequent color changing is neccessary, try using the tint value
	/// of the spritebatch draw instead for some performance gains)
	/// </summary>
	public AdvancedColor Foreground {
		get => foreground;
		set {
			foreground = value;
			NeedsVerts = true;
			NeedsRedraw = true;
		}
	}

	private int glyphLimit = -1;
	/// <summary>
	/// A sort of 'mask' that only allows a certain number of glyphs to be drawn,
	/// which allows partially covering some text
	/// IMPORTANT: while this is fast, it doesn't include whitespaces or any invisible characters
	/// if getting this to work properly is complex, just set the text manually
	/// A negative value means that no masking should be done, and all text should be displayed normally
	/// </summary>
	public int GlyphLimit {
		get => glyphLimit;
		set {
			glyphLimit = value;
			NeedsRedraw = true;
		}
	}
	/// <summary>
	/// The height of one line of text (in pixels) in this element
	/// </summary>
	public float LineHeight => TextRenderer.Fonts[font].Layout.Metrics.LineHeight * fontSize;

	public void UpdateLines() {
		lines = TextRenderer.CalculateLines(font, text, fontSize, width);
		cachedHeight = LineHeight * lines.Count;
		NeedsLines = false;
	}

	public void AllocArray() {
		Verts = new VertexSDFText[currentLimit * 4];
	}

	/// <summary>
	/// Resizes the output texture of this element if neccessary
	/// </summary>
	public void EnsureCorrectOutput() {
		if (Output == null || Output.Width != width || Output.Height != cachedHeight) {
			Output = new RenderTarget2D(
				Game.Graphics.GraphicsDevice,
				(int)Math.Ceiling(width),
				(int)Math.Ceiling(cachedHeight)
			);
		}
	}

	public void UpdateVerts() {
		if (NeedsLines) {
			UpdateLines();
		}
		EnsureCorrectOutput();
		TextRenderer.CalculateString(this);
		NeedsVerts = false;
	}

	private bool UpdateLimit() {
		int newLimit = 100 + text.Length - (text.Length % 100);
		if (newLimit == currentLimit) {
			return false;
		} else {
			currentLimit = newLimit;
			return true;
		}
	}

	public void Dispose() {
		if (Output != null) {
			Output.Dispose();
			Output = null;
		}
		Font = "";
	}
}
