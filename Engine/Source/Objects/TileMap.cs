using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Engine.Objects;

public class TileMap : GameObject {
	public List<Texture2D> Textures = new List<Texture2D>();

	public int CellSize { get; }
	public int[,] Map { get; }
	public int MapWidth { get; }
	public int MapHeight { get; }
	public int AbsoluteWidth => MapWidth * CellSize;
	public int AbsoluteHeight => MapHeight * CellSize;

	public TileMap(int width, int height, int cellSize) : base(Vector2.Zero) {
		MapWidth = width;
		MapHeight = height;
		CellSize = cellSize;
		Map = new int[MapWidth, MapHeight];
	}

	public override void Draw(GameTime gameTime) {
		float startX = Camera.Active!.Position.X % CellSize;
		float startY = Camera.Active.Position.Y % CellSize;

		for (float x = -startX; x < Camera.Active.ScreenWidth; x += CellSize) {
			for (float y = -startY; y < Camera.Active.ScreenHeight; y += CellSize) {
				Vector2 absPos = new Vector2(x, y) + Camera.Active.Position;
				Point tilePos = (absPos / CellSize).ToPoint();

				if (tilePos.X >= MapWidth || tilePos.Y >= MapHeight || tilePos.X < 0 || tilePos.Y < 0 || Map[tilePos.X, tilePos.Y] == -1) {
					continue;
				}
				Texture2D tex = Textures[Map[tilePos.X, tilePos.Y]];

				Game.SpriteBatch!.Draw(tex, new Vector2(x, y), new Rectangle(0, 0, CellSize, CellSize), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
			}
		}
	}
}
