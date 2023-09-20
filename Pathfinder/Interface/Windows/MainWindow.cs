using System;

using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;

using ImGuiNET;

using Pathfinder.Config;
using Pathfinder.Objects;
using Pathfinder.Objects.Data;
using Pathfinder.Interface.Components;
using Pathfinder.Interface.Widgets;
using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Interface.Windows; 

[LocalService]
public class MainWindow : Window, IDisposable {
	private readonly ConfigService _config;
	private readonly ObjectService _objects;
	private readonly PluginGui _gui;

	private readonly ResultsTable _table;
	private readonly RangeControls _range;
	private readonly FiltersPopup _filters;

	private IObjectClient? _client;
	
	public MainWindow(
		ConfigService _config,
		ObjectService _objects,
		PluginGui _gui,
		ResultsTable _table,
		RangeControls _range,
		FiltersPopup _filters
	) : base(
		"Pathfinder"
	) {
		this._config = _config;
		this._objects = _objects;
		this._gui = _gui;

		this._table = _table;
		this._range = _range;
		this._filters = _filters;
	}
	
	// Object access
	
	private IObjectClient GetClient()
		=> this._client ??= this._objects.CreateClient();
	
	// UI draw
	
	private const string FilterPopupId = "ObjectFilterPopup";
	private const string OverlayPopupId = "OverlayPopup";

	public override void PreDraw() {
		var displaySize = ImGui.GetIO().DisplaySize;
		this.Size = displaySize * 0.325f;
		this.SizeCondition = ImGuiCond.FirstUseEver;
		this.SizeConstraints = new WindowSizeConstraints {
			MinimumSize = displaySize * 0.1f,
			MaximumSize = displaySize
		};
	}

	public override void Draw() {
		var client = GetClient();
		var config = this._config.Get();
		DrawContextControls(config);
		ImGui.Spacing();
		DrawSearchFilter(config);
		this._table.Draw(client, config);
		DrawPopups(config);
	}
	
	// Context controls

	private void DrawContextControls(ConfigFile config) {
		ImGui.BeginGroup();
		
		ImGui.Checkbox(
			config.Overlay.Enabled ? "Overlay enabled" : "Overlay disabled",
			ref config.Overlay.Enabled
		);

		ImGui.SameLine(0, ImGui.GetStyle().ItemSpacing.X);
		ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 180f);
		if (Buttons.IconButton("##OverlayPopup", FontAwesomeIcon.EllipsisH))
			ImGui.OpenPopup(OverlayPopupId);
		ImGui.PopStyleVar();
		
		ImGui.SameLine(0, 0);

		const FontAwesomeIcon SettingsIcon = FontAwesomeIcon.Cog;
		var avail = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X;
		ImGui.SetCursorPosX(avail - Buttons.CalcIconButtonSize(SettingsIcon).X);
		if (Buttons.IconButton("##PathfindSettings", SettingsIcon))
			this._gui.GetWindow<ConfigWindow>().Toggle();
		
		ImGui.EndGroup();
		
		ImGui.Spacing();
		this._range.Draw(config);
	}
	
	// Results table

	private void DrawSearchFilter(ConfigFile config) {
		const FontAwesomeIcon FilterIcon = FontAwesomeIcon.Filter;
		const string FilterText = "Filters";

		var style = ImGui.GetStyle();
		var spacing = style.ItemInnerSpacing.X;
		var buttonWidth = spacing + style.CellPadding.X * 2 + ImGui.CalcTextSize(FilterText).X + UiBuilder.IconFont.FontSize;

		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - buttonWidth - spacing);
		ImGui.InputTextWithHint("##ObjectSearchString", "Search paths...", ref config.Filters.SearchString, 255);

		ImGui.SameLine(0, spacing);
		
		if (ImGuiComponents.IconButtonWithText(FilterIcon, FilterText))
			ImGui.OpenPopup(FilterPopupId);
	}
	
	// Popups

	private void DrawPopups(ConfigFile config) {
		ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, ImGui.GetStyle().WindowRounding);
		try {
			this._filters.Draw(FilterPopupId, config);
			DrawOverlayPopup(config);
		} finally {
			ImGui.PopStyleVar();
		}
	}

	private void DrawOverlayPopup(ConfigFile config) {
		if (!ImGui.BeginPopup(OverlayPopupId)) return;

		ImGui.Checkbox("Draw item dots", ref config.Overlay.ItemDot.Draw);
		
		ImGui.EndPopup();
	}
	
	// Window close

	public override void OnClose() {
		Dispose();
	}
	
	// Disposal

	public void Dispose() {
		this._client?.Dispose();
		this._client = null;
	}
}
