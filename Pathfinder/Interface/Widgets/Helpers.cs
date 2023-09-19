using ImGuiNET;

namespace Pathfinder.Interface.Widgets; 

public static class Helpers {
	public static bool HoverTooltip(string text) {
		var hover = ImGui.IsItemHovered();
		if (hover) ImGui.SetTooltip(text);
		return hover;
	}
}
