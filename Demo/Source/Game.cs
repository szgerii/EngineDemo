using Engine.Scenes;
using Engine.Debug;
using Engine.Input;
using Demo.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Engine.Graphics;

namespace Demo;

public class Game : Engine.Game {
	protected override void Initialize() {
		base.Initialize();
		//DisplayManager.SetResolution(1920, 1080);
		//DisplayManager.SetWindowType(WindowType.FULL_SCREEN);
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

		InputManager.Keyboard.OnPressed(Keys.V, () => DebugMode.ToggleFeature("coll-check-areas"));
		InputManager.Keyboard.OnPressed(Keys.C, () => DebugMode.ToggleFeature("coll-draw"));
		InputManager.Keyboard.OnPressed(Keys.G, () => DebugMode.ToggleFeature("draw-grid"));
	}
}
