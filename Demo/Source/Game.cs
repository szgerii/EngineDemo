using Engine;
using Engine.Scenes;
using Engine.Debug;
using Engine.Input;
using Engine.Graphics;
using Engine.UI;
using Demo.Scenes;
using Demo.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Demo;

public class Game : Engine.Game {
	private bool displayStats = false;
	private TextElement statsText;

	protected override void Initialize() {
		base.Initialize();
		InputSetup();
		GameScene scn = new();
		SceneManager.Load(scn);
		scn.LoadMap("desert");

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

		DebugMode.AddFeature(new ExecutedDebugFeature("toggle-stats", () => {
			displayStats = !displayStats;

			if (displayStats) {
				statsText = new("", "roboto", 40, 400, Color.Black);
			} else {
				statsText.Dispose();
			}
		}));

		DebugMode.Enable();
	}

	protected override void LoadContent() {
		base.LoadContent();

		// load fonts here (after TextRenderer has been initalized)
		TextRenderer.LoadFont("roboto");
	}

	private readonly PerformanceCalculator tickTime = new(50), drawTime = new(50);
	protected override void Update(GameTime gameTime) {
		DateTime start = DateTime.Now;
		base.Update(gameTime);
		tickTime.AddValue((DateTime.Now - start).TotalMilliseconds);
	}

	protected override void Draw(GameTime gameTime) {
		DateTime start = DateTime.Now;
		base.Draw(gameTime);
		drawTime.AddValue((DateTime.Now - start).TotalMilliseconds);

		if (displayStats) {
			statsText.Text = $"{tickTime.Average:0.00} ms / {drawTime.Average:0.00} ms\n{tickTime.Max:0.00} ms / {drawTime.Max:0.00} ms";

			SpriteBatch.Begin(sortMode: SpriteSortMode.BackToFront, samplerState: SamplerState.PointClamp);
			SpriteBatch.Draw(
				statsText.Output,
				Vector2.One * 20,
				new Rectangle(0, 0, statsText.Output.Width, statsText.Output.Height),
				Color.White,
				0,
				new Vector2(0, 0),
				1,
				SpriteEffects.None,
				.3f
			);
			SpriteBatch.End();
		}
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
		InputManager.Keyboard.OnPressed(Keys.P, () => DebugMode.Execute("toggle-stats"));
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
