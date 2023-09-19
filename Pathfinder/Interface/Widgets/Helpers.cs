using ImGuiNET;

namespace Pathfinder.Interface.Widgets; 

public static class Helpers {
	public static bool HoverTooltip(string text) {
		var hover = ImGui.IsItemHovered();
		if (hover) ImGui.SetTooltip(text);
		return hover;
	}
	
	public unsafe static bool DimColor(ImGuiCol col, float factor) {
		var ptr = ImGui.GetStyleColorVec4(col);
		if (ptr == null) return false;
		var value = *ptr;
		value.X *= factor;
		value.Y *= factor;
		value.Z *= factor;
		ImGui.PushStyleColor(col, value);
		return true;
	}
}
