using Engine;
using Engine.Debug;
using Engine.Input;
using Engine.Objects;
using Engine.Source.Interfaces;
using Microsoft.Xna.Framework;
using System;

namespace Safari.Components;

public class CameraControllerCmp : Component, IUpdatable {
	public static float DefaultScrollSpeed { get; set; } = 200f;
	public static float DefaultAcceleration { get; set; } = 23f;
	public static float DefaultZoom { get; set; } = 2;

	public float ScrollSpeed { get; set; } = DefaultScrollSpeed;
	public float ScrollAcceleration { get; set; } = DefaultAcceleration;
	public float ScrollDeceleration { get; set; } = 100f;

	private Vector2 currentSpeed = Vector2.Zero;

	public float ZoomSpeed { get; set; } = 0.05f;
	public float MinZoom { get; set; } = 0.55f;
	public float MaxZoom { get; set; } = 4f;

	public float FastModifier { get; set; } = 1.8f;
	public float SlowModifier { get; set; } = 0.5f;

	public bool CanBeDragged { get; set; } = false;
	public Action LockMouse { get; set; } = () => { };
	public Action UnlockMouse { get; set; } = () => { };
	public Func<bool> GetMouseLockState { get; set; } = () => false;

	public GameObject? TrackTarget { get; set; }

	public Rectangle? Bounds { get; set; } = null;

	private bool canEnterDragMode = false;
	private Camera Camera => (Camera)Owner!;

	public CameraControllerCmp() { }

	public CameraControllerCmp(Rectangle bounds, GameObject? trackTarget = null) {
		Bounds = bounds;
		TrackTarget = trackTarget;
	}

	public override void Load() {
		base.Load();
	}

	public void Update(GameTime gameTime) {
		if (!Engine.Game.CanDraw) {
			return;
		}

		Mouse mouse = InputManager.Mouse;
		if (mouse.JustPressed(MouseButtons.LeftButton)) {
			canEnterDragMode = true;
		} else if (mouse.IsUp(MouseButtons.LeftButton)) {
			canEnterDragMode = false;
			UnlockMouse();
		}

		if (DebugMode.IsFlagActive("no-drag")) {
			canEnterDragMode = false;
			UnlockMouse();
		}

		Vector2 prevPos = Owner!.Position;

		// pos
		if (TrackTarget != null) {
			Vector2 targetPos = TrackTarget is IHasCenter objWithCenter ? objWithCenter.Center : Owner!.Position;
			
			CenterOnPosition(targetPos);
		} else {
			CalcInputPan();
			Owner.Position += currentSpeed * (GetMouseLockState() ? 1f : (float)gameTime.ElapsedGameTime.TotalSeconds);
			if (prevPos != Utils.Round(Owner.Position).ToVector2()) {
				Owner.Position = Utils.Round(Owner.Position).ToVector2();
			}

			if (Owner.Position == prevPos) {
				currentSpeed = Vector2.Zero;
			}
		}

		// zoom
		Camera.Zoom += GetInputZoom(gameTime);
		Camera.Zoom = Math.Clamp(Camera.Zoom, MinZoom, MaxZoom);

		if (InputManager.Actions.JustPressed("reset-zoom")) {
			Camera.Zoom = DefaultZoom;
		}

		// clamp pos (it is taken care of automatically during object tracking)
		if (TrackTarget == null) {
			ClampToBounds();
		}
	}

	/// <summary>
	/// Calculates the camera movement delta vector based on the currently pressed inputs
	/// </summary>
	/// <returns>The result vector</returns>
	private void CalcInputPan() {
		bool mouseMovementOverThreshold = InputManager.Mouse.Movement.LengthSquared() >= 1f;

		if (canEnterDragMode && mouseMovementOverThreshold) {
			LockMouse();
		}

		if (GetMouseLockState()) {
			if (mouseMovementOverThreshold) {
				float xDiff = Camera.RealViewportSize.X / (Engine.Game.RenderTarget!.Width * Engine.Game.RenderTargetScale);
				float yDiff = Camera.RealViewportSize.Y / (Engine.Game.RenderTarget!.Height * Engine.Game.RenderTargetScale);

				currentSpeed = -InputManager.Mouse.Movement * new Vector2(xDiff, yDiff);
			} else {
				currentSpeed = Vector2.Zero;
			}

			return;
		}

		UnlockMouse();

		Vector2 delta = Vector2.Zero;

		// delta unit is 3 for axis aligned movement, 2 for diagonal
		// this is because camera movement is the smoothest if the camera position only uses ints
		// 3-2 is a closer pairing than the standard 1-sqrt(2), resulting in less precision loss during rounding

		if (InputManager.Actions.IsDown("left")) {
			delta.X -= 3;
		}
		if (InputManager.Actions.IsDown("right")) {
			delta.X += 3;
		}
		if (InputManager.Actions.IsDown("up")) {
			delta.Y -= 3;
		}
		if (InputManager.Actions.IsDown("down")) {
			delta.Y += 3;
		}

		bool noUserDelta = delta == Vector2.Zero;

		if (delta.X != 0 && delta.Y != 0) {
			delta.X = Math.Sign(delta.X) * 2;
			delta.Y = Math.Sign(delta.Y) * 2;
		}

		float speedMultiplier = IsFastModDown() ? FastModifier :
								IsSlowModDown() ? SlowModifier : 1f;

		if (ScrollAcceleration > 0 && (currentSpeed != Vector2.Zero || delta != Vector2.Zero)) {
			// acceleration enabled

			// apply deceleration to stale components
			// same 3-2 unit system as above

			if (delta.X == 0 && Math.Abs(currentSpeed.X) < 0.5f) {
				currentSpeed.X = 0f;
			}

			if (delta.Y == 0 && Math.Abs(currentSpeed.Y) < 0.5f) {
				currentSpeed.Y = 0f;
			}

			int sx = Math.Sign(currentSpeed.X);
			int sy = Math.Sign(currentSpeed.Y);
			if (delta.X == 0 && delta.Y == 0) {
				delta.X = -Math.Sign(currentSpeed.X) * 2;
				delta.Y = -Math.Sign(currentSpeed.Y) * 2;
			} else {
				if (delta.X == 0) {
					delta.X = -Math.Sign(currentSpeed.X) * 3;
				}

				if (delta.Y == 0) {
					delta.Y = -Math.Sign(currentSpeed.Y) * 3;
				}
			}

			// apply acceleration/deceleration to delta
			currentSpeed += delta * ScrollAcceleration;

			if (noUserDelta) {
				if (sx != Math.Sign(currentSpeed.X)) {
					currentSpeed.X = 0;
					delta.X = 0;
				}

				if (sy != Math.Sign(currentSpeed.Y)) {
					currentSpeed.Y = 0;
					delta.Y = 0;
				}
			}

			if (currentSpeed.Length() > 5 * ScrollSpeed * speedMultiplier) {
				currentSpeed = Vector2.Normalize(currentSpeed) * 5 * ScrollSpeed * speedMultiplier;
			}
		} else {
			// acceleration disabled

			if (delta != Vector2.Zero) {
				currentSpeed = delta * ScrollSpeed * speedMultiplier;
			} else {
				currentSpeed = Vector2.Zero;
			}
		}
	}

	/// <summary>
	/// Calculates the zoom delta from inputs
	/// </summary>
	/// <param name="gameTime">The current game time</param>
	/// <returns>The calculated zoom delta</returns>
	private float GetInputZoom(GameTime gameTime) {
		float scaleDelta = InputManager.Mouse.ScrollMovement;

		if (scaleDelta == 0f) {
			if (InputManager.Actions.IsDown("increase-zoom")) {
				scaleDelta += 10f;
			}
			if (InputManager.Actions.IsDown("decrease-zoom")) {
				scaleDelta -= 10f;
			}
		}

		scaleDelta *= ZoomSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * Camera.Zoom;

		if (IsFastModDown()) {
			scaleDelta *= FastModifier;
		}
		if (IsSlowModDown()) {
			scaleDelta *= SlowModifier;
		}

		return scaleDelta;
	}

	/// <summary>
	/// Centers the camera onto a given position
	/// </summary>
	/// <param name="position"></param>
	public void CenterOnPosition(Vector2 position) {
		Owner!.Position = position;

		ClampToBounds();
	}

	/// <summary>
	/// Ensures that the camera is inside the bounds of the controller
	/// </summary>
	private void ClampToBounds() {
		if (Bounds == null) return;

		Rectangle bounds = Bounds.Value;

		float camScale = 1f / Camera.Zoom;
		int realWidth = Utils.Round(bounds.Width - Camera.ScreenWidth * camScale);
		int realHeight = Utils.Round(bounds.Height - Camera.ScreenHeight * camScale);

		Point realSize = new Point(realWidth, realHeight);
		Rectangle realBounds = new Rectangle(bounds.Location + (Camera.RealViewportSize / 2f).ToPoint(), realSize);

		Owner!.Position = realBounds.Clamp(Owner.Position);
	}

	private bool IsFastModDown() {
		return InputManager.Actions.Exists("fast-mod") && InputManager.Actions.IsDown("fast-mod");
	}
	
	private bool IsSlowModDown() {
		return InputManager.Actions.Exists("slow-mod") && InputManager.Actions.IsDown("slow-mod");
	}
}
