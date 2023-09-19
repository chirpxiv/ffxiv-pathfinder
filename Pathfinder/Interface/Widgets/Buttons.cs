using System;
using System.Numerics;

using Dalamud.Interface;

using ImGuiNET;

namespace Pathfinder.Interface.Widgets; 

public static class Buttons {
	public static Vector2 CalcIconSize(FontAwesomeIcon icon) {
		ImGui.PushFont(UiBuilder.IconFont);
		try {
			return ImGui.CalcTextSize(icon.ToIconString());
		} finally {
			ImGui.PopFont();
		}
	}
    
	public static bool IconButton(string id, FontAwesomeIcon icon, Vector2? size = null) {
		ImGui.PushFont(UiBuilder.IconFont);
		try {
			size ??= Vector2.Zero;
			ImGui.BeginGroup();
			var cX = ImGui.GetCursorPosX();
			var result = ImGui.Button(id, size.Value);
            ImGui.SameLine(0, 0);
			ImGui.SetCursorPosX(cX + (size.Value.X - CalcIconSize(icon).X) / 2);
			ImGui.Text(icon.ToIconString());
			ImGui.EndGroup();
			return result;
		} finally {
			ImGui.PopFont();
		}
	}

	public static Vector2 CalcIconToggleSize(FontAwesomeIcon iconOn, FontAwesomeIcon iconOff) => new(
		Math.Max(
			CalcIconSize(iconOn).X,
			CalcIconSize(iconOff).X
		) + ImGui.GetStyle().ItemInnerSpacing.X,
		ImGui.GetFrameHeight()
	);

	public static bool IconToggleButton(string id, ref bool value, FontAwesomeIcon iconOn, FontAwesomeIcon iconOff) {
		var size = CalcIconToggleSize(iconOn, iconOff);
		var toggle = IconButton(id, value ? iconOn : iconOff, size);
		if (toggle) value = !value;
		return toggle;
	}
}
