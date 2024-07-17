using Microsoft.Xna.Framework;

namespace Engine.UI;

public abstract class UIObject : IDrawable {
	/// <summary>
	/// The UI object's position on the screen, from the top left corner
	/// </summary>
	public Vector2 Position { get; set; }
	/// <summary>
	/// The layer depth of the UI object, kept between 0-0.1 for now
	/// </summary>
	public float LayerDepth { get; set; }

	public UIObject(Vector2 pos, float layerDepth = 0) {
		Position = pos;
		LayerDepth = layerDepth;
	}

	/// <summary>
	/// Runs every frame, after every game object has been updated
	/// DO NOT ADD NEW UI OBJECTS TO THE GAME HERE (for now)
	/// </summary>
	public virtual void Update(GameTime gameTime) { }

	/// <summary>
	/// Draws the UI object to the screen
	/// When this is method is called, the GraphicsDevice is targeting the screen itself
	/// </summary>
	public abstract void Draw(GameTime gameTime);

	/// <summary>
	/// Gets called after the UI object has entered a loaded scene
	/// </summary>
	public virtual void Load() { }

	/// <summary>
	/// Gets called before the UI object leaves a loaded scene
	/// </summary>
	public virtual void Unload() { }
}