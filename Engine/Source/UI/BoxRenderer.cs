using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Engine.UI;

static class BoxRenderer {
	private static Effect boxEffect;
	private static DynamicVertexBuffer vbo;
	private static IndexBuffer ibo;
	private static HashSet<BoxElement> elements = new HashSet<BoxElement>();
	public static IReadOnlySet<BoxElement> Elements => elements;


	/// <summary>
	/// Call me in LoadContent() :point-right: :point-left:
	/// </summary>
	public static void Init(string effectPath) {
		boxEffect = Game.ContentManager.Load<Effect>(effectPath);
		vbo = new DynamicVertexBuffer(
			Game.Graphics.GraphicsDevice,
			VertexBox.VertexDeclaration,
			4,
			BufferUsage.WriteOnly
		);
		ibo = new IndexBuffer(
			Game.Graphics.GraphicsDevice,
			IndexElementSize.ThirtyTwoBits,
			6,
			BufferUsage.WriteOnly
		);
		int[] indices = new int[6];
		indices[0] = 0;
		indices[1] = 1;
		indices[2] = 2;
		indices[3] = 2;
		indices[4] = 3;
		indices[5] = 0;
		ibo.SetData(indices);
	}

	/// <summary>
	/// Pretty please at the start of Game.Draw(), BEFORE spritebatch.Begin tyy ~.~
	/// </summary>
	public static void Render() {
		GraphicsDevice device = Game.Graphics.GraphicsDevice;
		// bind ibo
		device.Indices = ibo;
		foreach (BoxElement elem in elements) {
			if (elem.NeedsRedraw) {
				elem.UpdateVerts();
				// RT
				device.SetRenderTarget(elem.Output);
				// clear
				device.Clear(Color.Transparent);
				// vbo
				vbo.SetData(elem.Verts, 0, 4);
				device.SetVertexBuffer(vbo);
				// apply the shader (+size uniform)
				float w = elem.Size.X;
				float h = elem.Size.Y;
				boxEffect.Parameters["ScreenSize"].SetValue(new Vector2(w, h));
				boxEffect.CurrentTechnique.Passes[0].Apply();
				// draw
				device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

				elem.NeedsRedraw = false;
			}
		}
		device.SetRenderTarget(null);
	}

	/// <summary>
	/// Manually add an element to the rendered boxes (done automatically)
	/// </summary>
	public static void AddElement(BoxElement element) {
		if (!elements.Contains(element)) {
			elements.Add(element);
		}
	}

	/// <summary>
	/// Manually remove an element from the rendered boxes
	/// </summary>
	public static void RemoveElement(BoxElement element) {
		if (elements.Contains(element)) {
			elements.Remove(element);
		}
	}

	public static void CalculateBox(BoxElement elem) {
		Vector2 horizontal = new Vector2(1, 0);
		Vector2 vertical = new Vector2(horizontal.Y, -horizontal.X);

		Vector2 topLeft = new Vector2(0, elem.Size.Y);
		Vector2 topRight = topLeft + (elem.Size.X * horizontal);
		Vector2 bottomLeft = topLeft + (elem.Size.Y * vertical);
		Vector2 bottomRight = bottomLeft + (elem.Size.X * horizontal);

		Vector2 topLeftLocal = new Vector2(0f, 0f);
		Vector2 topRightLocal = new Vector2(1f, 0f);
		Vector2 bottomRightLocal = new Vector2(1f, 1f);
		Vector2 bottomLeftLocal = new Vector2(0f, 1f);

		elem.Verts[0] = new VertexBox(
			new Vector3(topLeft, 0f),
			topLeftLocal,
			elem.BorderRadius,
			elem.BorderThickness,
			elem.Size,
			elem.BackgroundColor.TopLeft,
			elem.BorderColor.TopLeft
		);
		elem.Verts[1] = new VertexBox(
			new Vector3(topRight, 0f),
			topRightLocal,
			elem.BorderRadius,
			elem.BorderThickness,
			elem.Size,
			elem.BackgroundColor.TopRight,
			elem.BorderColor.TopRight
		);
		elem.Verts[2] = new VertexBox(
			new Vector3(bottomRight, 0f),
			bottomRightLocal,
			elem.BorderRadius,
			elem.BorderThickness,
			elem.Size,
			elem.BackgroundColor.BottomRight,
			elem.BorderColor.BottomRight
		);
		elem.Verts[3] = new VertexBox(
			new Vector3(bottomLeft, 0f),
			bottomLeftLocal,
			elem.BorderRadius,
			elem.BorderThickness,
			elem.Size,
			elem.BackgroundColor.BottomLeft,
			elem.BorderColor.BottomLeft
		);
	}
}

public struct VertexBox : IVertexType {
	public Vector3 position;
	public Vector2 localCoordinates;
	public float borderRadius;
	public float borderThickness;
	public Vector2 pixelSize;
	public Color backgroundColor;
	public Color borderColor;

	public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
		new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
		new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
		new VertexElement(20, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1),
		new VertexElement(24, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2),
		new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 3),
		new VertexElement(36, VertexElementFormat.Color, VertexElementUsage.Color, 0),
		new VertexElement(40, VertexElementFormat.Color, VertexElementUsage.Color, 1)
	);

	public VertexBox(Vector3 position, Vector2 localCoordinates, float borderRadius, float borderThickness, Vector2 pixelSize, Color backgroundColor, Color borderColor) {
		this.position = position;
		this.localCoordinates = localCoordinates;
		this.borderRadius = borderRadius;
		this.borderThickness = borderThickness;
		this.pixelSize = pixelSize;
		this.backgroundColor = backgroundColor;
		this.borderColor = borderColor;
	}

	VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
}
