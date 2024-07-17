using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Engine.UI;

/// <summary>
/// Stores information associated with a specific font
///
/// Don't create fonts manually, instead use TextRenderer.LoadFont(name)
/// </summary>
public class Font {
	public string Name { get; private set; }
	public Texture2D Texture { get; private set; }
	public AtlasLayout Layout { get; private set; }
	private Dictionary<char, Glyph> glyphs = new Dictionary<char, Glyph>();
	private Dictionary<(char, char), float> kerningPairs = new Dictionary<(char, char), float>();
	private HashSet<TextElement> elements = new HashSet<TextElement>();
	public IReadOnlyDictionary<char, Glyph> Glyphs => glyphs;
	public IReadOnlyDictionary<(char, char), float> KerningPairs => kerningPairs;
	public IReadOnlySet<TextElement> Elements => elements;

	/// <summary>
	/// Creates and loads a new font based on the given name
	/// (json layout and bmp texture file names should match this name)
	/// In most cases fonts shouldn't be created manually, isntead they should
	/// be loaded through the TextRenderer
	/// </summary>
	public Font(string name) {
		Name = name;
		string src = "assets/fonts/" + name;
		Texture = Game.ContentManager.Load<Texture2D>(src);
		using (StreamReader sr = new StreamReader("Content/" + src + ".json", Encoding.UTF8)) {
			string full = sr.ReadToEnd();
			JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
			Layout = JsonSerializer.Deserialize<AtlasLayout>(full, options);

			foreach (Glyph g in Layout.Glyphs) {
				glyphs.Add((char)g.Unicode, g);
			}
			foreach (KerningPair pair in Layout.Kerning) {
				kerningPairs.Add(((char)pair.Unicode1, (char)pair.Unicode2), pair.Advance);
			}
		}
	}

	/// <summary>
	/// Manually add a text element to this font (also done automatically in most cases)
	/// </summary>
	public void AddElement(TextElement element) {
		if (!elements.Contains(element)) {
			elements.Add(element);
		}
	}

	/// <summary>
	/// Manually remove a text element to this font (also done automatically in most cases)
	/// </summary>
	public void RemoveElement(TextElement element) {
		if (elements.Contains(element)) {
			elements.Remove(element);
		}
	}
}
