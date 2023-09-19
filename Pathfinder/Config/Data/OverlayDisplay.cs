namespace Pathfinder.Config.Data; 

public class OverlayDisplay {
	public bool DrawAll = true;
	public bool DrawMin = true;
	public bool DrawMax = true;
	public bool DrawLineTo = true;
        
	public uint MinColor = 0x6FFFFFFF;
	public uint MaxColor = 0x8FFFFFFF;
	public uint LineToColor = 0xFFFFFFFF;
}
