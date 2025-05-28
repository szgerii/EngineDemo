using Engine;
using Engine.Components;
using Engine.Graphics.Stubs.Texture;
using Microsoft.Xna.Framework;

namespace Demo.Objects;

public class Tile : GameObject {
	public Tile(Vector2 pos, ITexture2D tex, float ySortOffset = 0, float layerDepth = 0.5f) : base(pos) {
		SpriteCmp sprite = new(tex) {
			YSortEnabled = true,
			YSortOffset = ySortOffset,
			LayerDepth = layerDepth
		};
		Attach(sprite);
	}
}
