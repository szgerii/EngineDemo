using Engine.Scenes;
using Engine.Debug;
using Engine.Input;
using Demo.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Demo;

public class Game : Engine.Game {
	protected override void Initialize() {
		base.Initialize();

		InputSetup();
		GameScene scn = new();
		SceneManager.Load(scn);
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
		InputManager.Keyboard.OnPressed(Keys.G, () => DebugMode.ToggleFeature("map-grid"));
		InputManager.Keyboard.OnPressed(Keys.R, () => DebugMode.ToggleFeature("generate-level"));
	}
}
