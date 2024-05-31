using Engine.Scenes;
using Engine.Debug;
using Engine.Input;
using Demo.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Engine.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Engine;
using Demo.Components;

namespace Demo;

public class Game : Engine.Game {
	static Game() {
		DebugMode.AddFeature(new ExecutedDebugFeature("toggle-fullscreen", () => {
			if (DisplayManager.WindowType == WindowType.FULL_SCREEN) {
				DisplayManager.SetWindowType(WindowType.WINDOWED, false);
				DisplayManager.SetResolution(1280, 720, false);
			} else {
				DisplayManager.SetWindowType(WindowType.FULL_SCREEN, false);
				DisplayMode nativeRes = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
				DisplayManager.SetResolution(nativeRes.Width, nativeRes.Height, false);
			}
			DisplayManager.ApplyChanges();
		}));
	}

	protected override void Initialize() {
		base.Initialize();
		InputSetup();
		GameScene scn = new();
		SceneManager.Load(scn);
		scn.LoadMap("desert");
		DebugMode.Enable();
	}

	protected override void LoadContent() {
		base.LoadContent();
	}

	protected override void Update(GameTime gameTime) {
		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime) {
		base.Draw(gameTime);
	}

	private void InputSetup() {
		InputManager.Actions.Register("up", new InputAction(keys: new[] { Keys.W, Keys.Up }));
		InputManager.Actions.Register("down", new InputAction(keys: new[] { Keys.S, Keys.Down }));
		InputManager.Actions.Register("left", new InputAction(keys: new[] { Keys.A, Keys.Left }));
		InputManager.Actions.Register("right", new InputAction(keys: new[] { Keys.D, Keys.Right }));

		// debug
		InputManager.Keyboard.OnPressed(Keys.V, () => DebugMode.ToggleFeature("coll-check-areas"));
		InputManager.Keyboard.OnPressed(Keys.C, () => DebugMode.ToggleFeature("coll-draw"));		
		InputManager.Keyboard.OnPressed(Keys.G, () => DebugMode.ToggleFeature("draw-grid"));
		InputManager.Keyboard.OnPressed(Keys.F, () => DebugMode.Execute("toggle-fullscreen"));
		InputManager.Keyboard.OnPressed(Keys.H, () => DebugMode.ToggleFeature("draw-health"));
		InputManager.Keyboard.OnPressed(Keys.X, () => {
			foreach (GameObject obj in GameScene.Active.GameObjects) {
				if (obj.GetComponent(out HealthCmp h)) {
					h.Health -= 1;
				}
			}
		});
		InputManager.Keyboard.OnPressed(Keys.Y, () => {
			foreach (GameObject obj in GameScene.Active.GameObjects) {
				if (obj.GetComponent(out HealthCmp h)) {
					h.Health += 1;
				}
			}
		});
	}
}
