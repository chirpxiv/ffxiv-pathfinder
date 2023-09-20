using System;
using System.Numerics;

using Dalamud.Interface;
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
			DrawTab("Colors", DrawColorsTab, cfg);
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

		ImGui.Checkbox("Override dot color", ref dot.ColorOverride);

		Vector4 col4;
		if (dot.ColorOverride) {
			col4 = ImGui.ColorConvertU32ToFloat4(dot.Color);
			if (ImGui.ColorEdit4($"Dot Color##{text}", ref col4))
				dot.Color = ImGui.ColorConvertFloat4ToU32(col4);
		}
		
		col4 = ImGui.ColorConvertU32ToFloat4(dot.OutlineColor);
		if (ImGui.ColorEdit4($"Outline Color##{text}", ref col4))
			dot.OutlineColor = ImGui.ColorConvertFloat4ToU32(col4);
		
		ImGui.SliderFloat($"Outline Width##{text}", ref dot.Width, 0.0f, 10.0f);
		
		ImGui.SliderFloat($"Dot Radius##{text}", ref dot.Radius, 1.0f, 20.0f);
	}
	
	// Tabs: Colors

	private void DrawColorsTab(ConfigFile cfg) {
		ImGui.Checkbox("Color object table", ref cfg.Table.UseColors);
		var ov = !cfg.Overlay.ItemDot.ColorOverride;
		if (ImGui.Checkbox("Color overlay dots", ref ov))
			cfg.Overlay.ItemDot.ColorOverride = !ov;
		
		ImGui.Spacing();
		
		ImGui.Text("Objects:");
		DrawColor(cfg, WorldObjectType.Terrain);
		DrawColor(cfg, WorldObjectType.BgObject);
		
		ImGui.Spacing();
		
		ImGui.Text("Characters:");
		DrawColor(cfg, WorldObjectType.Human);
		DrawColor(cfg, WorldObjectType.DemiHuman);
		DrawColor(cfg, WorldObjectType.Monster);
		DrawColor(cfg, WorldObjectType.Weapon);
	}

	private void DrawColor(ConfigFile cfg, WorldObjectType key) {
		if (ResetToDefault($"##Reset_{key}"))
			cfg.Colors[key] = ConfigFile.DefaultColors[key];

		var color = cfg.GetColor(key);
		var col4 = ImGui.ColorConvertU32ToFloat4(color);
		var col3 = new Vector3(col4.X, col4.Y, col4.Z);
		if (ImGui.ColorEdit3(key.ToString(), ref col3))
			cfg.Colors[key] = ImGui.ColorConvertFloat4ToU32(new Vector4(col3, 1));
	}

	private bool ResetToDefault(string id) {
		var shift = ImGui.IsKeyDown(ImGuiKey.ModShift);
		ImGui.BeginDisabled(!shift);
		var result = Buttons.IconButton(id, FontAwesomeIcon.Undo);
		ImGui.EndDisabled();
		ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
		Helpers.HoverTooltip("Reset to default (Hold shift to press)");
		return result;
	}
	
	// Tabs: Advanced

	private void DrawAdvancedTab(ConfigFile cfg) {
		ImGui.Checkbox("Developer Mode", ref cfg.Table.ShowAddress);
		ImGui.SameLine();
		Helpers.Hint("When enabled, displays memory addresses for objects in the results table.");
	}
}
