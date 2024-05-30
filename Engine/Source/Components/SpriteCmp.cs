﻿using Engine.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Components;

/// <summary>
/// Component for drawing simple sprites on screen
/// </summary>
public class SpriteCmp : Component, IDrawable {
	#region Properties
	/// <summary>
	/// The texture of the sprite
	/// </summary>
	public Texture2D Texture { get; set; }
	/// <summary>
	/// The origin point of the sprite
	/// </summary>
	public Vector2 Origin { get; set; }
	/// <summary>
	/// The rotation of the sprite in radians
	/// </summary>
	public float Rotation { get; set; }
	/// <summary>
	/// The scale used for drawing the sprite
	/// </summary>
	public float Scale { get; set; }
	/// <summary>
	/// The flip effects to apply when drawing the sprite
	/// </summary>
	public SpriteEffects Flip { get; set; }
	/// <summary>
	/// The source rectangle of the sprite (inside Texture)
	/// </summary>
	public Rectangle? SourceRectangle { get; set; }
	/// <summary>
	/// The base layer depth used when determining the order of things on the screen
	/// </summary>
	public float LayerDepth { get; set; }
	/// <summary>
	/// Whether or not Y-Sort should be used for this object
	/// This means that if another object is above the sprite, it can behind it
	/// </summary>
	public bool YSortEnabled { get; set; } = false;
	/// <summary>
	/// The amount of empty space between the bottom of the texture and its actual content
	/// This is used for determining the actual Y position of the sprite
	/// </summary>
	public float YSortOffset { get; set; } = 0f;
	/// <summary>
	/// The layer depth of the sprite, accounting for Y sorting
	/// </summary>
	public virtual float RealLayerDepth {
		get {
			if (!YSortEnabled) {
				return LayerDepth;
			}

			return LayerDepth - 0.1f * ((Owner.ScreenPosition.Y + YSortOffset) / Camera.Active.ScreenHeight);
		}
	}
	/// <summary>
	/// Whether or not the sprite should be rendered to the screen
	/// </summary>
	public bool Visible { get; set; } = true;
#endregion

	public SpriteCmp(Texture2D texture, Vector2? origin = null, float rotation = 0, float scale = 1, SpriteEffects flip = SpriteEffects.None, Rectangle? sourceRectangle = null, float layerDepth = 1) {
		Texture = texture;
		Origin = origin ?? Vector2.Zero;
		Rotation = rotation;
		Scale = scale;
		Flip = flip;
		SourceRectangle = sourceRectangle;
		LayerDepth = layerDepth;
	}

	public virtual void Draw(GameTime gameTime) {
		if (Visible) {
			Game.SpriteBatch.Draw(Texture, Owner.ScreenPosition, SourceRectangle, Color.White, Rotation, Origin, Scale, Flip, RealLayerDepth);
		}
	}
}
