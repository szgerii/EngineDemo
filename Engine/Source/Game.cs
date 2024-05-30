﻿using Engine.Audio;
using Engine.Scenes;
using Engine.Input;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Engine;

public class Game : Microsoft.Xna.Framework.Game {
	public static Game Instance { get; protected set; }
	public static GraphicsDeviceManager Graphics { get; protected set; }
	public static SpriteBatch SpriteBatch { get; protected set; }
	public static RenderTarget2D RenderTarget { get; protected set; }
	public static ContentManager ContentManager => Instance.Content;

	#region SHORTHANDS
	public static void AddObject(GameObject obj) => SceneManager.Active.AddObject(obj);
	public static void RemoveObject(GameObject obj) => SceneManager.Active.RemoveObject(obj);
	#endregion

	protected GraphicsDeviceManager _graphics;
	protected SpriteBatch _spriteBatch;

	public Game() {
		Instance = this;
		_graphics = new GraphicsDeviceManager(this);
		Graphics = _graphics;

		IsMouseVisible = true;
	}

	protected override void Initialize() {
		InputManager.Initialize();
		AudioManager.Initialize();
		Content.RootDirectory = "Content";

		DisplayManager.Init();
		DisplayManager.SetResolution(640, 360);
		RenderTarget = new RenderTarget2D(GraphicsDevice, 640, 360);

		base.Initialize();
	}

	protected override void LoadContent() {
		_spriteBatch = new SpriteBatch(GraphicsDevice);
		SpriteBatch = _spriteBatch;
	}

	protected override void Update(GameTime gameTime) {
		InputManager.Update(gameTime);

		if (SceneManager.HasLoadingScheduled) {
			SceneManager.PerformScheduledLoad();
		}
		
		SceneManager.Active.PerformObjectRemovals();
		SceneManager.Active.PerformObjectAdditions();

		SceneManager.Active.Update(gameTime);

		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime) {
		float scale = Graphics.PreferredBackBufferHeight / (float)RenderTarget.Height;
		
		GraphicsDevice.SetRenderTarget(RenderTarget);
		GraphicsDevice.Clear(Color.Black);

		SpriteBatch.Begin(sortMode: SpriteSortMode.BackToFront, samplerState: SamplerState.PointClamp);
		SceneManager.Active.Draw(gameTime);
		SpriteBatch.End();
		
		GraphicsDevice.SetRenderTarget(null);
		GraphicsDevice.Clear(Color.Black);

		SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
		SpriteBatch.Draw(RenderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
		SpriteBatch.End();

		base.Draw(gameTime);
	}
}