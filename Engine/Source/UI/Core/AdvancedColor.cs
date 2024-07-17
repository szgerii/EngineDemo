using Microsoft.Xna.Framework;

namespace Engine.UI.Core;

/// <summary>
/// The direction of a gradient
/// (a direction of top-left for example means that
/// the gradient will move from the bottom-right to the top-left corner of the shape)
/// </summary>
public enum GradientDirection {
	Right,
	Left,
	Up,
	Down,
	TopLeft,
	TopRight,
	BottomLeft,
	BottomRight
}

public struct AdvancedColor {
	#region Instance members
	public Color TopLeft;
	public Color TopRight;
	public Color BottomLeft;
	public Color BottomRight;

	public AdvancedColor(Color topLeft, Color topRight, Color bottomLeft, Color bottomRight) {
		TopLeft = topLeft;
		TopRight = topRight;
		BottomLeft = bottomLeft;
		BottomRight = bottomRight;
	}
	#endregion

	#region Static methods
	public static implicit operator AdvancedColor(Color color) => Fill(color);

	/// <summary>
	/// Will draw the quad using a linear gradient
	/// </summary>
	public static AdvancedColor Gradient(Color from, Color to, GradientDirection dir, Vector2 size) {
		switch (dir) {
			case GradientDirection.Up:
				return new AdvancedColor(to, to, from, from);
			case GradientDirection.Down:
				return new AdvancedColor(from, from, to, to);
			case GradientDirection.Left:
				return new AdvancedColor(to, from, to, from);
			case GradientDirection.Right:
				return new AdvancedColor(from, to, from, to);
			case GradientDirection.BottomRight: {
					Color topLeft = from;
					Color bottomRight = to;
					(float slice1, float slice2) = DiagonalRelativeSlices(size);
					Color bottomLeft = Color.Lerp(from, to, slice1);
					Color topRight = Color.Lerp(from, to, slice2);
					return new AdvancedColor(topLeft, topRight, bottomLeft, bottomRight);
				}
			case GradientDirection.TopLeft: {
					Color bottomRight = from;
					Color topLeft = to;
					(float slice1, float slice2) = DiagonalRelativeSlices(size);
					Color topRight = Color.Lerp(to, from, slice1);
					Color bottomLeft = Color.Lerp(to, from, slice2);
					return new AdvancedColor(topLeft, topRight, bottomLeft, bottomRight);
				}
			case GradientDirection.BottomLeft: {
					Color topRight = from;
					Color bottomLeft = to;
					(float slice1, float slice2) = DiagonalRelativeSlices(size);
					Color bottomRight = Color.Lerp(from, to, slice1);
					Color topLeft = Color.Lerp(from, to, slice2);
					return new AdvancedColor(topLeft, topRight, bottomLeft, bottomRight);
				}
			case GradientDirection.TopRight: {
					Color bottomLeft = from;
					Color topRight = to;
					(float slice1, float slice2) = DiagonalRelativeSlices(size);
					Color topLeft = Color.Lerp(to, from, slice1);
					Color bottomRight = Color.Lerp(to, from, slice2);
					return new AdvancedColor(topLeft, topRight, bottomLeft, bottomRight);
				}
			default:
				return new AdvancedColor(from, from, from, from);
		}
	}

	/// <summary>
	/// Will draw the quad with one solid color
	/// (consider using the implicit ctor)
	/// </summary>
	public static AdvancedColor Fill(Color color) {
		return new AdvancedColor(color, color, color, color);
	}

	private static (float, float) DiagonalRelativeSlices(Vector2 size) {
		float sum = size.Length();
		float ratio = (size.X * size.Y) / (size.Y * size.Y);
		// x-es = ratio * y-os
		// ratio * y-os + y-os = sum
		// masik * (ratio + 1) = sum
		float szam1 = sum / (ratio + 1);
		float szam2 = sum - szam1;

		return (szam1 / sum, szam2 / sum);
	}
	#endregion
}
