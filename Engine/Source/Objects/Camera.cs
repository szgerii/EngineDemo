using Engine.Helpers;
using Microsoft.Xna.Framework;
using System;

namespace Engine.Objects;

public class Camera : GameObject {
	public static Camera? Active { get; set; }

	public float Zoom { get; set; } = 1.0f;

	private float rotation = 0.0f;
	public float Rotation {
		get => rotation;
		set {
			rotation = value % (float)(2 * Math.PI);
		}
	}

	public Matrix TransformMatrix { get; private set; }

	public Point ScreenSize => Game.RenderTarget!.Bounds.Size;
	public int ScreenWidth => ScreenSize.X;
	public int ScreenHeight => ScreenSize.Y;
	public Vector2 RealViewportSize => ScreenSize.ToVector2() * (1f / Zoom);
	public Vectangle RealViewportBounds => new Vectangle(Position - (RealViewportSize / 2f), RealViewportSize);

	public Camera(Vector2? position = null) : base(position ?? Vector2.Zero) { }

	public override void Update(GameTime gameTime) {
		base.Update(gameTime);

		TransformMatrix =
			Matrix.CreateTranslation(new Vector3(-Position, 0)) *
			Matrix.CreateRotationZ(Rotation) *
			Matrix.CreateScale(Zoom, Zoom, 1f) *
			Matrix.CreateTranslation(ScreenWidth / 2f, ScreenHeight / 2f, 0);
	}
}
