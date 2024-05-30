using Engine.Scenes;
using Engine.Collision;
using Engine.Objects;
using Demo.Objects;
using Microsoft.Xna.Framework;

namespace Demo.Scenes;

public class GameScene : Scene {
	public override void Load() {
		int mapWidth = 30, mapHeight = 20, cellSize = 32;

		CollisionManager.Init(mapWidth, mapHeight, cellSize);
		PostUpdate += CollisionManager.PostUpdate;

		Player player = new Player(Vector2.One * 150);
		AddObject(player);

		Camera.Active = new Camera(player, new Rectangle(0, 0, mapWidth * cellSize, mapHeight * cellSize));
		AddObject(Camera.Active);

		base.Load();
	}

	public override void Unload() {
		PostUpdate -= CollisionManager.PostUpdate;
		
		base.Unload();
	}
}
