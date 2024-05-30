using Microsoft.Xna.Framework;

namespace Engine.Objects;

public class Camera : GameObject {
	public static Camera Active { get; set; }

	public Point ScreenSize => new Point(Game.RenderTarget.Width, Game.RenderTarget.Height);
	public int ScreenWidth => ScreenSize.X;
	public int ScreenHeight => ScreenSize.Y;

	/// <summary>
	/// The min and max values the camera's position is allowed to take
	/// </summary>
	public Rectangle? Bounds { get; set; }

	/// <summary>
	/// The object the camera should be following
	/// </summary>
	public GameObject Target { get; set; }

	public Camera(GameObject target = null, Rectangle? bounds = null, Vector2? position = null) : base(position ?? Vector2.Zero) {
		Bounds = bounds;
		Target = target;
	}

	public override void Update(GameTime gameTime) {
		if (Target != null) {
			Position = Target.Position - new Vector2(ScreenSize.X, ScreenSize.Y) / 2;
		}

		if (Bounds != null) {
			Point minPos = Bounds.Value.Location;
			Point maxPos = new Point(Bounds.Value.Right, Bounds.Value.Bottom) - ScreenSize;
			Position = Vector2.Clamp(Position, minPos.ToVector2(), maxPos.ToVector2());
		}
	}
}
