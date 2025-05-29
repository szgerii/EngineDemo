using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Engine.UI.Core;

public static class TextRenderer {
	private readonly static Dictionary<string, Font> fonts = new Dictionary<string, Font>();
	public static IReadOnlyDictionary<string, Font> Fonts => fonts;
	private static Effect? textEffect;
	private static DynamicVertexBuffer? vbo;
	private static IndexBuffer? ibo;
	private static int bufferLen;

	/// <summary>
	/// Initializes the text renderer, call in LoadContent() pretty please
	/// </summary>
	public static void Init(string effectPath, int bufferSize = 800) {
		if (!Game.CanDraw) return;
		
		textEffect = Game.ContentManager.Load<Effect>(effectPath);
		vbo = new DynamicVertexBuffer(
			Game.Graphics.GraphicsDevice,
			VertexSDFText.VertexDeclaration,
			bufferSize * 4,
			BufferUsage.WriteOnly
		);
		ibo = new IndexBuffer(
			Game.Graphics.GraphicsDevice,
			IndexElementSize.ThirtyTwoBits,
			bufferSize * 6,
			BufferUsage.WriteOnly
		);
		int[] indices = new int[bufferSize * 6];
		for (int i = 0; i < bufferSize; i++) {
			int currentIndexPoint = 6 * i;
			int currentVertexPoint = 4 * i;
			indices[currentIndexPoint] = currentVertexPoint;
			indices[currentIndexPoint + 1] = currentVertexPoint + 1;
			indices[currentIndexPoint + 2] = currentVertexPoint + 2;
			indices[currentIndexPoint + 3] = currentVertexPoint + 2;
			indices[currentIndexPoint + 4] = currentVertexPoint + 3;
			indices[currentIndexPoint + 5] = currentVertexPoint + 0;
		}
		ibo.SetData(indices);
		bufferLen = 4 * bufferSize;
	}

	/// <summary>
	/// Load a font with the specified name
	///
	/// The name (also used as an identifier for the font) should match the filename of
	/// both the msdf texture, and the layout json file
	///
	/// Eg. Times New Roman would be stored in
	/// Content/assets/fonts/times.bmp and times.json
	/// and the id/name used would be 'times'
	/// </summary>
	/// <param name="name"></param>
	public static void LoadFont(string name) {
		fonts.Add(name, new Font(name));
	}

	/// <summary>
	/// Call at the start of Game.Draw(), BEFORE spritebatch.Begin pls ^^
	/// </summary>
	public static void Render() {
		if (!Game.CanDraw) return;

		GraphicsDevice device = Game.Graphics.GraphicsDevice;
		// bind ibo
		device.Indices = ibo;
		foreach ((string name, Font font) in fonts) {
			// font specific uniforms
			textEffect!.Parameters["PxRange"].SetValue(font.Layout.Atlas!.DistanceRange);
			textEffect.Parameters["AtlasSize"].SetValue(new Vector2(font.Layout.Atlas.Width, font.Layout.Atlas.Height));
			textEffect.Parameters["AtlasTexture"].SetValue(font.Texture);
			foreach (TextElement elem in font.Elements) {
				if (elem.NeedsRedraw) {
					if (elem.NeedsVerts) {
						elem.UpdateVerts();
					}
					int vertCount = elem.VertexCount;
					if (elem.GlyphLimit >= 0) {
						vertCount = Math.Min(vertCount, elem.GlyphLimit * 4);
					}
					// RT
					device.SetRenderTarget(elem.Output);
					// clear
					device.Clear(Color.Transparent);
					int currentStart = 0;
					int length = vertCount - currentStart;
					if (length > bufferLen) {
						length = bufferLen;
					}
					while (length > 0) {
						// vbo
						vbo!.SetData(elem.Verts, currentStart, length);
						device.SetVertexBuffer(vbo);
						// apply the shader (+size uniform)
						float w = elem.Width;
						float h = elem.Height;
						textEffect.Parameters["ScreenSize"].SetValue(new Vector2(w, h));
						textEffect.CurrentTechnique.Passes[0].Apply();
						// draw
						device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, length / 2);

						currentStart += length;
						length = vertCount - currentStart;
						if (length > bufferLen) {
							length = bufferLen;
						}
					}
					elem.NeedsRedraw = false;
				}
			}
		}
		device.SetRenderTarget(null);
	}
	/// <summary>
	/// Calculate the vertices of the text (DO NOT TOUCH)
	/// </summary>
	public static void CalculateString(TextElement elem) {
		Font font = fonts[elem.Font];
		elem.VertexCount = 0;

		Vector2 horizontal = new Vector2(1, 0);
		Vector2 vertical = new Vector2(horizontal.Y, -horizontal.X);

		Vector2 pen = new Vector2(0, elem.Height);

		pen += vertical * elem.FontSize * font.Layout.Metrics!.Ascender;

		float fontMultiplier = elem.FontSize / 64;
		Vector2 newLinePos = pen;
		float minimumSpaceAdvance = font.Glyphs[' '].Advance * elem.FontSize;

		Vector2 topLeftLocal = new Vector2(0f, 0f);
		Vector2 topRightLocal = new Vector2(1f, 0f);
		Vector2 bottomRightLocal = new Vector2(1f, 1f);
		Vector2 bottomLeftLocal = new Vector2(0f, 1f);

		foreach (TextLine line in elem.Lines) {
			if (elem.TextAlign == TextAlign.Right) {
				pen += (horizontal * (elem.Width - line.TotalAdvance));
			} else if (elem.TextAlign == TextAlign.Center) {
				pen += (horizontal * (elem.Width - line.TotalAdvance)) / 2;
			}
			float spaceAdvance = minimumSpaceAdvance;
			if (elem.TextAlign == TextAlign.Justify && !line.IsParagraphEnd) {
				spaceAdvance += (elem.Width - line.TotalAdvance) / (float)line.Spaces;
			}

			for (int i = line.StartIndex; i < line.StartIndex + line.Length; i++) {
				char c = elem.Text[i];
				Glyph g = font.Glyphs[c];
				if (c == '\r') {
					continue;
				}
				if (c == ' ') {
					pen += horizontal * spaceAdvance;
					continue;
				}

				if (!char.IsWhiteSpace(c)) {
					float sampLeft = g.AtlasBounds!.Left;
					float sampRight = g.AtlasBounds.Right;
					float sampTop = font.Layout.Atlas!.Height - g.AtlasBounds.Top;
					float sampBottom = font.Layout.Atlas.Height - g.AtlasBounds.Bottom;
					float originalGlyphHeight = sampBottom - sampTop;
					float originalGlyphWidth = sampRight - sampLeft;

					float glyphHeight = originalGlyphHeight * fontMultiplier;
					float glyphWidth = originalGlyphWidth * fontMultiplier;
					Vector2 originalGlyphSize = new Vector2(originalGlyphWidth, originalGlyphHeight);

					float left = pen.X + (g.PlaneBounds!.Left * elem.FontSize);
					float bottom = pen.Y + (g.PlaneBounds.Bottom * elem.FontSize);
					float right = left + glyphWidth;
					float top = bottom + glyphHeight;

					Vector2 bottomLeft = pen + (horizontal * g.PlaneBounds.Left * elem.FontSize) - (vertical * g.PlaneBounds.Bottom * elem.FontSize);
					Vector2 bottomRight = bottomLeft + horizontal * glyphWidth;
					Vector2 topLeft = bottomLeft - vertical * glyphHeight;
					Vector2 topRight = topLeft + horizontal * glyphWidth;

					elem.Verts![elem.VertexCount] = new VertexSDFText(
						new Vector3(topLeft, 0f),
						elem.Foreground.TopLeft,
						new Vector2(sampLeft, sampTop),
						topLeftLocal,
						originalGlyphSize
					);
					elem.Verts[elem.VertexCount + 1] = new VertexSDFText(
						new Vector3(topRight, 0f),
						elem.Foreground.TopRight,
						new Vector2(sampRight, sampTop),
						topRightLocal,
						originalGlyphSize
					);
					elem.Verts[elem.VertexCount + 2] = new VertexSDFText(
						new Vector3(bottomRight, 0f),
						elem.Foreground.BottomRight,
						new Vector2(sampRight, sampBottom),
						bottomRightLocal,
						originalGlyphSize
					);
					elem.Verts[elem.VertexCount + 3] = new VertexSDFText(
						new Vector3(bottomLeft, 0f),
						elem.Foreground.BottomLeft,
						new Vector2(sampLeft, sampBottom),
						bottomLeftLocal,
						originalGlyphSize
					);
					elem.VertexCount += 4;
				}

				pen += horizontal * g.Advance * elem.FontSize;

				if (i < elem.Text.Length - 1) {
					if (font.KerningPairs.ContainsKey((elem.Text[i], elem.Text[i + 1]))) {
						pen += horizontal * font.KerningPairs[(elem.Text[i], elem.Text[i + 1])] * elem.FontSize;
					}
				}
			}
			pen = newLinePos;
			pen += vertical * font.Layout.Metrics.LineHeight * elem.FontSize;
			newLinePos = pen;
		}
	}

	/// <summary>
	/// DO NOT TOUCH
	/// </summary>
	public static List<TextLine> CalculateLines(string fontName, string text, float fontSize, float boxWidth) {
		List<TextLine> lines = new List<TextLine>();
		TextLine currentLine = new TextLine(0);
		Font font = fonts[fontName];
		float minimumSpaceAdvance = font.Glyphs[' '].Advance * fontSize;
		int wordLength = 0;
		float wordAdvance = 0f;

		for (int i = 0; i < text.Length + 1; i++) {
			if (i == text.Length || text[i] == ' ') {
				// Space, new word
				if (currentLine.TotalAdvance + wordAdvance < boxWidth) {
					// Word fits in the current line
					if (!currentLine.IsEmpty) {
						currentLine.Length++;
						currentLine.TotalAdvance += minimumSpaceAdvance;
						currentLine.Spaces++;
					}
					currentLine.Length += wordLength;
					currentLine.TotalAdvance += wordAdvance;
					currentLine.IsEmpty = false;
					wordLength = 0;
					wordAdvance = 0f;
				} else {
					// Line full, new line
					if (!currentLine.IsEmpty) {
						lines.Add(currentLine);
						currentLine = new TextLine(i - wordLength);
						currentLine.Length += wordLength;
						currentLine.TotalAdvance += wordAdvance;
						currentLine.IsEmpty = false;
						wordLength = 0;
						wordAdvance = 0f;
					} else {
						currentLine.Length += wordLength;
						currentLine.TotalAdvance += wordAdvance;
						currentLine.IsEmpty = false;
						wordLength = 0;
						wordAdvance = 0f;
						lines.Add(currentLine);
						currentLine = new TextLine(i - wordLength);
					}
				}
				continue;
			}
			char c = text[i];
			if (c == '\n') {
				// New line pff
				currentLine.IsParagraphEnd = true;
				if (currentLine.TotalAdvance + wordAdvance < boxWidth) {
					// The current word fits
					if (!currentLine.IsEmpty) {
						currentLine.Length++;
						currentLine.TotalAdvance += minimumSpaceAdvance;
						currentLine.Spaces++;
					}
					currentLine.Length += wordLength;
					currentLine.TotalAdvance += wordAdvance;
					currentLine.IsEmpty = false;
					wordLength = 0;
					wordAdvance = 0f;
					lines.Add(currentLine);
					currentLine = new TextLine(i + 1);
				} else if (!currentLine.IsEmpty) {
					// The current word does not fit in the line
					lines.Add(currentLine);
					currentLine = new TextLine(i - wordLength);
					currentLine.Length += wordLength;
					currentLine.TotalAdvance += wordAdvance;
					currentLine.IsEmpty = false;
					wordLength = 0;
					wordAdvance = 0f;
					lines.Add(currentLine);
					currentLine = new TextLine(i + 1);
				} else {
					currentLine.Length += wordLength;
					currentLine.TotalAdvance += wordAdvance;
					currentLine.IsEmpty = false;
					wordLength = 0;
					wordAdvance = 0f;
					lines.Add(currentLine);
					currentLine = new TextLine(i + 1);
				}
				continue;
			}
			if (c == '\r') {
				wordLength++;
				continue;
			}

			// Regular character, continue the current word
			Glyph g = font.Glyphs[c];
			float advance = g.Advance * fontSize;
			if (i < text.Length - 1) {
				if (font.KerningPairs.ContainsKey((text[i], text[i + 1]))) {
					advance += font.KerningPairs[(text[i], text[i + 1])] * fontSize;
				}
			}
			wordAdvance += advance;
			wordLength++;
		}
		currentLine.IsParagraphEnd = true;
		lines.Add(currentLine);
		return lines;
	}
}

/// <summary>
/// Stores data associated with one line of text
/// </summary>
public struct TextLine {
	/// <summary>
	/// The index of the first character in this line
	/// </summary>
	public int StartIndex { get; set; }
	/// <summary>
	/// The length (in characters) of this line
	/// </summary>
	public int Length { get; set; } = 0;
	/// <summary>
	/// The number of spaces in this line
	/// </summary>
	public int Spaces { get; set; } = 0;
	/// <summary>
	/// Stores whether this line is the final line
	/// in a paragraph or not (useful for justified alignment)
	/// </summary>
	public bool IsParagraphEnd { get; set; } = false;
	/// <summary>
	/// Storer whether this line has any characters or not
	/// </summary>
	public bool IsEmpty { get; set; } = true;
	/// <summary>
	/// The total length (in pixels) of this line (including spaces)
	/// </summary>
	public float TotalAdvance { get; set; } = 0;

	public TextLine(int startIndex) {
		StartIndex = startIndex;
	}
}

/// <summary>
/// The vertex type used for rendering SDF glyph quads (for text)
/// contains
///	- a vec3 of floats for position (required)
///	- a vec4 of bytes for color
///	- a vec2 of floats for texture coordinates (atlas wide)
///	- a vec2 of floats for texture coordinates (local)
///	- a vec2 of floats for glyph size
///	all of this adds up to a total of 40 bytes per vertex -> 160 bytes per quad
/// </summary>
public struct VertexSDFText : IVertexType {
	public Vector3 position;
	public Color color;
	public Vector2 textureCoordinates;
	public Vector2 localCoordinates;
	public Vector2 glyphSize;

	public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
		new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
		new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
		new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
		new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
		new VertexElement(32, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 2)
	);

	public VertexSDFText(Vector3 position, Color color, Vector2 textureCoordinates, Vector2 localCoordinates, Vector2 glyphSize) {
		this.position = position;
		this.color = color;
		this.textureCoordinates = textureCoordinates;
		this.localCoordinates = localCoordinates;
		this.glyphSize = glyphSize;
	}

	readonly VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
}
