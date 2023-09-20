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
		Color = 0xFFFFFFFF,
		OutlineColor = 0xFF000000,
		Radius = 8.0f,
		Width = 1.5f
	};
}

public class OverlayElement {
	public bool Draw = true;
	public uint Color = 0xFFFFFFFF;
	public float Width = 2.0f;
}

public class OverlayDotElement : OverlayElement {
	public bool ColorOverride;
	
	public float Radius;
	public uint OutlineColor = 0xFF000000;
}
