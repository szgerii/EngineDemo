using Engine.Scenes;
using Engine.Collision;
using Microsoft.Xna.Framework;
using System;
using Engine.Debug;
using Engine.UI;
using Engine.UI.Core;
using Engine.Helpers;
using Engine.Objects;

namespace Demo.Scenes;

public class GameScene : Scene {
	public static GameScene Active => SceneManager.Active as GameScene;

	public Level CurrentLevel { get; private set; }

	static GameScene() {
		DebugMode.AddFeature(new LoopedDebugFeature("draw-grid", (object _, GameTime _) => Active?.CurrentLevel.DrawGrid(), GameLoopStage.POST_DRAW));
	}

	public override void Unload() {
		PostUpdate -= CollisionManager.PostUpdate;
		RemoveObject(CurrentLevel);
		
		base.Unload();
	}

	// TODO: unload previous map
	public void LoadMap(string mapName) {
		if (SceneManager.Active != this) {
			throw new InvalidOperationException("Trying to load a map through an inactive game scene");
		}

		Camera.Active = new Camera(Vector2.Zero);
		Camera.Active.Zoom = 1.5f;
		Game.AddObject(Camera.Active);

		CurrentLevel = new Level(mapName);
		CurrentLevel.LoadLevel();
		AddObject(CurrentLevel);
		InitMap();
	}

	private void InitMap() {
		CollisionManager.Init(new Vectangle(0, 0, CurrentLevel.MapWidth * CurrentLevel.TileSize, CurrentLevel.MapHeight * CurrentLevel.TileSize));
		PostUpdate += CollisionManager.PostUpdate;
	}
}
