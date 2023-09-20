using System;

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
		DrawCircleSettings("Outer circle display (Max range)", ref cfg.Overlay.Max);
		ImGui.Spacing();
		DrawCircleSettings("Inner circle display (Min range)", ref cfg.Overlay.Min);
	}
	
	private void DrawCircleSettings(string text, ref OverlayElement data) {
		ImGui.Checkbox(text, ref data.Draw);

		var col4 = ImGui.ColorConvertU32ToFloat4(data.Color);
		if (ImGui.ColorEdit4($"Color##{text}", ref col4))
			data.Color = ImGui.ColorConvertFloat4ToU32(col4);
		
		ImGui.SliderFloat($"Width##{text}", ref data.Width, 1.0f, 10.0f);
	}
	
	// Tabs: Advanced

	private void DrawAdvancedTab(ConfigFile cfg) {
		ImGui.Checkbox("Developer Mode", ref cfg.Table.ShowAddress);
		ImGui.SameLine();
		Helpers.Hint("When enabled, displays memory addresses for objects in the results table.");
	}
}
