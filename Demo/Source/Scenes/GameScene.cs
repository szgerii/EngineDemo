using Engine.Scenes;
using Engine.Collision;
using Microsoft.Xna.Framework;
using System;
using Engine.Debug;

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

		CurrentLevel = new Level(mapName);
		CurrentLevel.LoadLevel();
		AddObject(CurrentLevel);
		InitMap();
	}

	private void InitMap() {
		CollisionManager.Init(CurrentLevel.MapWidth, CurrentLevel.MapHeight, CurrentLevel.TileSize);
		PostUpdate += CollisionManager.PostUpdate;
	}
}
