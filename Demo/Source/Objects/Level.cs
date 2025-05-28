using Demo.Components;
using Demo.Objects;
using Engine;
using Engine.Collision;
using Engine.Components;
using Engine.Graphics.Stubs.Texture;
using Engine.Helpers;
using Engine.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using TiledCS;

namespace Demo;

public class Level : GameObject {
	public string MapName { get; private set; }
	public int TileSize { get; private set; }
	public int MapWidth { get; private set; }
	public int MapHeight { get; private set; }

	private List<(float layerDepth, Texture2D tex)> staticLayers;
	private Texture2D debugGridTex;

	public Level(string mapName) : base(Vector2.Zero) {
		MapName = mapName;
	}

	public void LoadLevel() {
		TiledMap map = new(Game.ContentManager.RootDirectory + "/assets/maps/" + MapName + ".tmx");
		// maps the first gids of tilesets to the actual tileset data
		Dictionary<int, TiledTileset> tilesets = map.GetTiledTilesets(Game.ContentManager.RootDirectory + "/assets/tilesets/");
		TiledLayer[] tileLayers = map.Layers.Where(e => e.type == TiledLayerType.TileLayer).ToArray();
		Dictionary<string, Color[]> rawTilesets = new();

		TileSize = map.TileWidth;
		MapWidth = map.Width;
		MapHeight = map.Height;
		staticLayers = new List<(float, Texture2D)>();

		float layerDepth = 1;
		for (int i = 0; i < tileLayers.Length; i++, layerDepth -= 0.01f) {
			TiledLayer layer = tileLayers[i];
			bool staticLayer = layer.properties.Any(e => e.name == "static" && e.value == "true");

			Color[] layerColorData;
			if (staticLayer) {
				layerColorData = new Color[layer.width * layer.height * TileSize * TileSize];
			} else {
				layerColorData = Array.Empty<Color>();
			}

			for (int y = 0; y < layer.height; y++) {
				for (int x = 0; x < layer.width; x++) {
					int index = (y * layer.width) + x,
						gid   = layer.data[index],
						tileX = x * TileSize,
						tileY = y * TileSize;

					if (gid == 0)
						continue;

					TiledMapTileset mapTileset = map.GetTiledMapTileset(gid);
					TiledTileset tileset = tilesets[mapTileset.firstgid];
					TiledSourceRect srcRect = map.GetSourceRect(mapTileset, tileset, gid);
					if (!rawTilesets.ContainsKey(tileset.Name)) {
						Texture2D tilesetTex = Game.ContentManager.Load<Texture2D>("assets/tilesets/" + tileset.Name);
						Color[] colorData = new Color[tilesetTex.Width * tilesetTex.Height];
						tilesetTex.GetData(colorData);
						rawTilesets[tileset.Name] = colorData;
					}
					Color[] tilesetColorData = rawTilesets[tileset.Name];

					Color[] tileColorData;
					if (staticLayer) {
						tileColorData = Array.Empty<Color>();
					} else {
						tileColorData = new Color[TileSize * TileSize];
					}

					for (int xOffset = 0; xOffset < TileSize; xOffset++) {
						for (int yOffset = 0; yOffset < TileSize; yOffset++) {
							Color targetPixel = tilesetColorData[(srcRect.y + yOffset) * tileset.TileWidth * tileset.Columns + srcRect.x + xOffset];

							if (staticLayer) {
								layerColorData[(tileY + yOffset) * MapWidth * TileSize + tileX + xOffset] = targetPixel;
							} else {
								tileColorData[yOffset * TileSize + xOffset] = targetPixel;
							}
						}
					}

					if (!staticLayer) {
						Texture2D tileTex = new(Game.Graphics.GraphicsDevice, TileSize, TileSize);
						tileTex.SetData(tileColorData);

						float ySortOffset = TileSize;
						for (int k = tileColorData.Length - 1; k >= 0; k--) {
							if (tileColorData[k] != Color.Transparent) {
								ySortOffset = k / TileSize;
								break;
							}
						}

						Tile tileObj = new(new Vector2(tileX, tileY), new Texture2DAdapter(tileTex), ySortOffset: ySortOffset);

						// collision
						TiledTile tile = map.GetTiledTile(mapTileset, tileset, gid);
						if (tile != null && tile.objects.Length > 0) {
							ColliderCollectionCmp collCmp = new();
							tileObj.Attach(collCmp);
							foreach (TiledObject obj in tile.objects) {
								Vectangle bounds = new(Utils.Round(obj.x), Utils.Round(obj.y), Utils.Round(obj.width), Utils.Round(obj.height));
								CollisionCmp collider = new(bounds) {
									Tags = CollisionTags.World
								};
								collCmp.AddCollider(collider);
							}
						}

						// destroyable
						if (tile != null && tile.properties.Any(e => e.name == "destroyable" && e.value == "true")) {
							HealthCmp healthCmp = new HealthCmp(3);
							tileObj.Attach(healthCmp);
						}

						Game.AddObject(tileObj);
					}
				}
			}

			if (staticLayer) {
				Texture2D layerTex = new(Game.Graphics.GraphicsDevice, MapWidth * TileSize, MapHeight * TileSize);
				layerTex.SetData(layerColorData);
				staticLayers.Add((layerDepth, layerTex));
			}
		}

		TiledLayer[] objectLayers = map.Layers.Where(e => e.type == TiledLayerType.ObjectLayer).ToArray();
		foreach (TiledLayer layer in objectLayers) {
			foreach (TiledObject obj in layer.objects) {
				if (obj.name == "player") {
					Player player = new Player(new Vector2(obj.x, obj.y));
					Game.AddObject(player);

					Rectangle maxBounds = Rectangle.Empty;
					foreach ((_, Texture2D tex) in staticLayers) {
						if (tex.Bounds.Width * tex.Bounds.Height > maxBounds.Width * maxBounds.Height) {
							maxBounds = tex.Bounds;
						}
					}

					CameraControllerCmp controller = new(maxBounds, player);
					Camera.Active.Attach(controller);
				}
			}
		}

		GenerateDebugGrid();
	}

	public override void Draw(GameTime gameTime) {
		foreach ((float depth, Texture2D tex) in staticLayers) {
			Game.SpriteBatch.Draw(tex, Vector2.Zero, tex.Bounds, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, depth);
        }

        base.Draw(gameTime);
	}

	public void DrawGrid() {
		Game.SpriteBatch.Draw(debugGridTex, -Camera.Active.Position, debugGridTex.Bounds, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.01f);
	}

	private void GenerateDebugGrid() {
		debugGridTex = new Texture2D(Game.Graphics.GraphicsDevice, MapWidth * TileSize, MapHeight * TileSize);
		Color[] colorData = new Color[debugGridTex.Width * debugGridTex.Height];

		for (int y = 0; y < MapHeight * TileSize; y++) {
			for (int x = 0; x < MapWidth * TileSize; x++) {
				Color c;
				if (x % TileSize == 0 || y % TileSize == 0 || x == MapWidth * TileSize - 1 || y == MapHeight * TileSize - 1) {
					c = Color.Black;
				} else {
					c = Color.Transparent;
				}

				colorData[y * MapWidth * TileSize + x] = c;
			}
		}

		debugGridTex.SetData(colorData);
	}
}
