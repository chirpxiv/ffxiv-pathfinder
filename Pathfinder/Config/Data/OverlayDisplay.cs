namespace Pathfinder.Config.Data; 

public class OverlayDisplay {
	public bool Enabled = true;

	public OverlayElement Min = new() {
		Color = 0x6FFFFFFF,
		Width = 1.5f
	};

	public OverlayElement Max = new() {
		Color = 0x8FFFFFFF,
		Width = 2.0f
	};
}

public class OverlayElement {
	public bool Draw = true;
	public uint Color = 0xFFFFFFFF;
	public float Width = 2.0f;
}
