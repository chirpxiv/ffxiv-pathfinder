using System;
using System.Numerics;

using Dalamud.Interface.Windowing;

using ImGuiNET;

using Pathfinder.Config;
using Pathfinder.Config.Data;
using Pathfinder.Interface.Widgets;
using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Interface.Windows; 

[LocalService]
public class ConfigWindow : Window {
	// Constructor

	private readonly ConfigService _cfg;

	public ConfigWindow(ConfigService _cfg) : base("Pathfinder Settings") {
		this._cfg = _cfg;
	}
	
	// Draw UI

	public override void PreDraw() {
		var displaySize = ImGui.GetIO().DisplaySize;
		this.SizeConstraints = new WindowSizeConstraints {
			MinimumSize = displaySize * 0.1f,
			MaximumSize = displaySize
		};
	}

	public override void Draw() {
        var cfg = this._cfg.Get();
		
		if (ImGui.BeginTabBar("##PfConfigTabs")) {
			DrawTab("Overlay", DrawOverlayTab, cfg);
			DrawTab("Advanced", DrawAdvancedTab, cfg);
			ImGui.EndTabBar();
		}
	}
	
	// Tabs

	private void DrawTab(string name, Action<ConfigFile> draw, ConfigFile cfg) {
		if (!ImGui.BeginTabItem(name)) return;
		
		try {
			ImGui.Spacing();
			draw.Invoke(cfg);
		} finally {
			ImGui.EndTabItem();
		}
	}
	
	// Tabs: Overlay

	private void DrawOverlayTab(ConfigFile cfg) {
		ImGui.Text("Radius circle:");
		
		DrawCircleSettings("Display outer circle (Max range)", ref cfg.Overlay.Max);
		ImGui.Spacing();
		DrawCircleSettings("Display inner circle (Min range)", ref cfg.Overlay.Min);
		
		ImGui.Spacing();
		ImGui.Spacing();
		ImGui.Text("Object dots:");
		
		DrawDotSettings("Display object dots", ref cfg.Overlay.ItemDot);
		
		// Hack to prevent dumb window proportions on first open.
		ImGui.SameLine();
		ImGui.Dummy(ImGui.GetItemRectSize() with { Y = 0 });
	}
	
	private void DrawCircleSettings(string text, ref OverlayElement data) {
		ImGui.Checkbox(text, ref data.Draw);

		var col4 = ImGui.ColorConvertU32ToFloat4(data.Color);
		if (ImGui.ColorEdit4($"Color##{text}", ref col4))
			data.Color = ImGui.ColorConvertFloat4ToU32(col4);
		
		ImGui.SliderFloat($"Width##{text}", ref data.Width, 1.0f, 10.0f);
	}
	
	private void DrawDotSettings(string text, ref OverlayDotElement dot) {
		ImGui.Checkbox(text, ref dot.Draw);

		var col4 = ImGui.ColorConvertU32ToFloat4(dot.Color);
		if (ImGui.ColorEdit4($"Dot Color##{text}", ref col4))
			dot.Color = ImGui.ColorConvertFloat4ToU32(col4);
		
		col4 = ImGui.ColorConvertU32ToFloat4(dot.OutlineColor);
		if (ImGui.ColorEdit4($"Outline Color##{text}", ref col4))
			dot.OutlineColor = ImGui.ColorConvertFloat4ToU32(col4);
		
		ImGui.SliderFloat($"Outline Width##{text}", ref dot.Width, 0.0f, 10.0f);
		
		ImGui.SliderFloat($"Dot Radius##{text}", ref dot.Radius, 1.0f, 20.0f);
	}
	
	// Tabs: Advanced

	private void DrawAdvancedTab(ConfigFile cfg) {
		ImGui.Checkbox("Developer Mode", ref cfg.Table.ShowAddress);
		ImGui.SameLine();
		Helpers.Hint("When enabled, displays memory addresses for objects in the results table.");
	}
}
