using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.UI.Core;

public class BoxElement {
	public bool NeedsRedraw { get; set; } = true;
	public VertexBox[] Verts { get; set; }
	public RenderTarget2D? Output { get; private set; }

	public BoxElement(Vector2 size, float borderRadius, float borderThickness, AdvancedColor backgroundColor, AdvancedColor borderColor) {
		this.size = size;
		this.borderThickness = borderThickness;
		this.borderRadius = borderRadius;
		this.backgroundColor = backgroundColor;
		this.borderColor = borderColor;
		Verts = new VertexBox[4];
		BoxRenderer.AddElement(this);
	}

	private Vector2 size;
	/// <summary>
	/// The size of the box (in pixels)
	/// </summary>
	public Vector2 Size {
		get => size;
		set {
			size = value;
			NeedsRedraw = true;
		}
	}

	private float borderRadius;
	/// <summary>
	/// How long the curve on the corners of the box should be (in pixels)
	/// </summary>
	public float BorderRadius {
		get => borderRadius;
		set {
			borderRadius = value;
			NeedsRedraw = true;
		}
	}

	private float borderThickness;
	/// <summary>
	/// The thickness of the border (in pixels)
	/// Warning: when using border radius always use a border
	/// (even if it's the same color as the background)
	/// </summary>
	public float BorderThickness {
		get => borderThickness;
		set {
			borderThickness = value;
			NeedsRedraw = true;
		}
	}

	private AdvancedColor backgroundColor;
	/// <summary>
	/// An xna color or a gradient used to color the background of the box
	/// </summary>
	public AdvancedColor BackgroundColor {
		get => backgroundColor;
		set {
			backgroundColor = value;
			NeedsRedraw = true;
		}
	}

	private AdvancedColor borderColor;
	/// <summary>
	/// An xna color or a gradient used to color the color of the border
	/// </summary>
	public AdvancedColor BorderColor {
		get => borderColor;
		set {
			borderColor = value;
			NeedsRedraw = true;
		}
	}

	/// <summary>
	/// Resizes the output texture of this element if neccessary
	/// </summary>
	public void EnsureCorrectOutput() {
		if (Game.CanDraw && (Output == null || Output.Width != size.X || Output.Height != size.Y)) {
			Output = new RenderTarget2D(
				Game.Graphics!.GraphicsDevice,
				(int)Math.Ceiling(size.X),
				(int)Math.Ceiling(size.Y)
			);
		}
	}

	public void UpdateVerts() {
		EnsureCorrectOutput();
		EnsureCorrectOutput();
		BoxRenderer.CalculateBox(this);
	}

	public void Dispose() {
		BoxRenderer.RemoveElement(this);
		if (Output != null) {
			Output.Dispose();
			Output = null;
		}
	}
}
