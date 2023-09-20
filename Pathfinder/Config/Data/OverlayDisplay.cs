namespace Pathfinder.Config.Data; 

public class OverlayDisplay {
	public bool Enabled = true;

	public OverlayElement Min = new() {
		Color = 0xAFFFFFFF,
		Width = 2.0f
	};

	public OverlayElement Max = new() {
		Color = 0xCFFFFFFF,
		Width = 2.5f
	};

	public OverlayDotElement ItemDot = new() {
		Radius = 8.0f,
		Width = 1.5f
	};

	public OverlayColElement HoverLine = new() {
		Width = 6.0f
	};
}

public class OverlayElement {
	public bool Draw = true;
	public uint Color = 0xFFFFFFFF;
	public float Width = 2.0f;
}

public class OverlayColElement : OverlayElement {
	public bool ColorOverride;
}

public class OverlayDotElement : OverlayColElement {
	public float Radius;
	public uint OutlineColor = 0xFF000000;
	public bool DimOnHover = true;
}
