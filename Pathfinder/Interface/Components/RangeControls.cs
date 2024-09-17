using Dalamud.Interface;

using ImGuiNET;

using Pathfinder.Config;
using Pathfinder.Config.Data;
using Pathfinder.Interface.Widgets;
using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Interface.Components; 

[LocalService(ServiceFlags.Transient)]
public class RangeControls {
	public void Draw(ConfigFile config) {
		var uiEnabled = config.Overlay.Enabled;
		
		this.DrawRadiusControl(
			"##MaxRadius",
			ref config.Filters.MaxRadius,
			ref config.Overlay.Max.Draw,
			"< %0.3f",
			"Max Range",
			"Toggle the maximum range requirement.\nDisabling this may cause visual clutter and lag in large areas!",
			"Toggle rendering for the outer line.",
			uiEnabled
		);
		
		this.DrawRadiusControl(
			"##MinRadius",
			ref config.Filters.MinRadius,
			ref config.Overlay.Min.Draw,
			"> %0.3f",
			"Min Range",
			"Toggle the minimum range requirement.\nThis is helpful if you don't want to see your own character, weapons, etc.",
			"Toggle rendering for the inner line.",
			uiEnabled
		);
	}

	private bool DrawRadiusControl(
		string id,
		ref FilterConstraint control,
		ref bool draw,
		string fmt = "%0.3f",
		string slideText = "",
		string? toggleTooltip = null,
		string? drawTooltip = null,
		bool uiEnabled = true
	) {
		const FontAwesomeIcon OnIcon = FontAwesomeIcon.Eye;
		const FontAwesomeIcon OffIcon = FontAwesomeIcon.EyeSlash;
		
		var spacing = ImGui.GetStyle().ItemInnerSpacing.X;
		
		ImGui.BeginGroup();

		var dragWidth = ImGui.GetFontSize() * 3.5f;
		var drawWidth = Buttons.CalcIconToggleSize(OnIcon, OffIcon).X;
		
		// Toggle checkbox
		ImGui.Checkbox($"{id}_Toggle", ref control.Enabled);
		if (toggleTooltip != null)
			Helpers.HoverTooltip(toggleTooltip);
		
		ImGui.BeginDisabled(!control.Enabled);
		ImGui.SameLine(0, spacing);
		
		// Value slider
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - dragWidth - drawWidth - spacing * 2);
		var changed = ImGui.SliderFloat($"{id}_Slider", ref control.Value, 0.01f, 100.0f, slideText, ImGuiSliderFlags.NoInput);
		ImGui.SameLine(0, spacing);

		// Value drag
		ImGui.SetNextItemWidth(dragWidth);
		ImGui.DragFloat($"{id}_Drag", ref control.Value, 0.01f, 0.01f, 100.0f, fmt);
		ImGui.SameLine(0, spacing);
		
		// Overlay toggle
		ImGui.BeginDisabled(!uiEnabled);
		Buttons.IconToggleButtonColored($"{id}_Toggle_Draw", ref draw, OnIcon, OffIcon);
		if (drawTooltip != null)
			Helpers.HoverTooltip(drawTooltip);
		ImGui.EndDisabled();
		
		ImGui.EndDisabled();
		
		ImGui.EndGroup();
		return changed;
	}
}
