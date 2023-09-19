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
			size ??= CalcIconButtonSize(icon);
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
	
	public static Vector2 CalcIconButtonSize(FontAwesomeIcon icon) {
		var height = ImGui.GetFrameHeight();
		return new Vector2(
			Math.Max(CalcIconSize(icon).X, height),
			height
		);
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
	
	public unsafe static bool IconToggleButtonColored(string id, ref bool value, FontAwesomeIcon iconOn, FontAwesomeIcon iconOff) {
		var i = 0;
		if (!value) i = DimColor(ImGuiCol.Text, 0.90f)
			+ DimColor(ImGuiCol.Button, 0.75f)
			+ DimColor(ImGuiCol.ButtonActive, 0.75f)
			+ DimColor(ImGuiCol.ButtonHovered, 0.75f);
		
		try {
			return IconToggleButton(id, ref value, iconOn, iconOff);
		} finally {
			if (i > 0) ImGui.PopStyleColor(i);
		}
	}

	private static int DimColor(ImGuiCol col, float factor)
		=> Helpers.DimColor(col, factor) ? 1 : 0;
}
