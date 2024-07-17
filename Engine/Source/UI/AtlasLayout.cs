namespace Engine.UI;

/// <summary>
/// Objects needed for JSON deserialization
/// </summary>
public class AtlasLayout {
	public Atlas Atlas { get; set; }
	public Metrics Metrics { get; set; }
	public Glyph[] Glyphs { get; set; }
	public KerningPair[] Kerning { get; set; }
}

public class Atlas {
	public string Type { get; set; }
	public int DistanceRange { get; set; }
	public int Size { get; set; }
	public int Width { get; set; }
	public int Height { get; set; }
	public string YOrigin { get; set; }
}

public class Metrics {
	public int EmSize { get; set; }
	public float LineHeight { get; set; }
	public float Ascender { get; set; }
	public float UnderlineY { get; set; }
	public float UnderlineThickness { get; set; }
}

public class Glyph {
	public int Unicode { get; set; }
	public float Advance { get; set; }
	public PlaneBounds PlaneBounds { get; set; }
	public AtlasBounds AtlasBounds { get; set; }
}

public class PlaneBounds {
	public float Left { get; set; }
	public float Bottom { get; set; }
	public float Right { get; set; }
	public float Top { get; set; }
}

public class AtlasBounds {
	public float Left { get; set; }
	public float Bottom { get; set; }
	public float Right { get; set; }
	public float Top { get; set; }
}

public class KerningPair {
	public int Unicode1 { get; set; }
	public int Unicode2 { get; set; }
	public float Advance { get; set; }
}
