using System;
using System.Linq;
using System.Collections.Generic;

using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;

using ImGuiNET;

using Pathfinder.Config;
using Pathfinder.Config.Data;
using Pathfinder.Interface.Widgets;
using Pathfinder.Objects;
using Pathfinder.Objects.Data;
using Pathfinder.Services;
using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Interface.Windows; 

[LocalService]
public class MainWindow : Window, IDisposable {
	private readonly ConfigService _config;
	private readonly ObjectService _objects;

	private readonly ChatService _chat;

	private IObjectClient? _client;
	
	public MainWindow(ConfigService _config, ObjectService _objects, ChatService _chat) : base(
		"Pathfinder"
	) {
		this._config = _config;
		this._objects = _objects;

		this._chat = _chat;

		var displaySize = ImGui.GetIO().DisplaySize;
		this.Size = displaySize * 0.325f;
		this.SizeCondition = ImGuiCond.FirstUseEver;
		this.SizeConstraints = new WindowSizeConstraints {
			MinimumSize = displaySize * 0.1f,
			MaximumSize = displaySize
		};
	}
	
	// Object access
	
	private IObjectClient GetClient()
		=> this._client ??= this._objects.CreateClient();
	
	// UI draw

	private const string SettingsPopupId = "PathfindSettingsPopup";
	private const string FilterPopupId = "ObjectFilterPopup";

	public override void Draw() {
		var config = this._config.Get();
		DrawContextControls(config);
		ImGui.Spacing();
		DrawSearchFilter(config);
		DrawObjectTable();
		DrawPopups(config);
	}
	
	// Context controls

	private void DrawContextControls(ConfigFile config) {
        ImGui.BeginGroup();
		
		ImGui.Checkbox(
			config.Overlay.Enabled ? "Overlay enabled" : "Overlay disabled",
			ref config.Overlay.Enabled
		);
		
		ImGui.SameLine(0, 0);

		var avail = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X;

		const FontAwesomeIcon SettingsIcon = FontAwesomeIcon.Cog;
		ImGui.SetCursorPosX(avail - Buttons.CalcIconButtonSize(SettingsIcon).X);
		if (Buttons.IconButton("##PathfindSettings", SettingsIcon))
			ImGui.OpenPopup(SettingsPopupId);
		
		ImGui.EndGroup();
		
		ImGui.Spacing();
		DrawRadiusControls(config);
	}
	
	// Radius controls

	private void DrawRadiusControls(ConfigFile config) {
		var uiEnabled = config.Overlay.Enabled;
		
		DrawRadiusControl(
			"##MaxRadius",
			ref config.Filters.MaxRadius,
			ref config.Overlay.Max.Draw,
			"< %0.3f",
			"Max Range",
			"Toggle the maximum range requirement.\nDisabling this may cause visual clutter and lag in large areas!",
			"Toggle rendering for the outer line.",
			uiEnabled
		);
		
		DrawRadiusControl(
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

	private void DrawObjectTable() {
		ImGui.BeginChildFrame(0x0B75, ImGui.GetContentRegionAvail());
		
		ImGui.BeginTable("##ObjectSearchTable", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable | ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable);
		
		var avail = ImGui.GetContentRegionAvail().X;
		ImGui.TableSetupColumn("Distance", ImGuiTableColumnFlags.DefaultSort, avail * 0.125f);
		ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.None, avail * 0.175f);
		ImGui.TableSetupColumn("Paths", ImGuiTableColumnFlags.None, avail * 0.7f);
		
		var objects = GetClient().GetObjects().ToList();
		SortTable(ImGui.TableGetSortSpecs().Specs, objects);
		
		ImGui.TableHeadersRow();

		foreach (var info in objects) {
			ImGui.TableNextRow();
			
			ImGui.TableSetColumnIndex(0);
			ImGui.Text($"{info.Distance:0.00}");
			
			ImGui.TableSetColumnIndex(1);
			ImGui.Text(info.GetItemTypeString());

			ImGui.TableSetColumnIndex(2);
            
			var count = info.Models.Count;
			if (count > 1) {
				ImGui.PushID($"Object_{info.Address:X}");

				var imKey = ImGui.GetID("Expand");
				var state = ImGui.GetStateStorage();
				var isExpand = state.GetBool(imKey);
                
				if (DrawColumnSelect($"{count} Entries (Click to {(isExpand ? "collapse" : "expand")})", isExpand)) {
					isExpand = !isExpand;
					state.SetBool(imKey, isExpand);
				}
                
				if (isExpand) DrawModelList(info);
			} else {
				var text = info.Models.FirstOrDefault()?.Path ?? string.Empty;
				DrawPath(text);
			}
		}
		
		ImGui.EndTable();
		
		ImGui.EndChildFrame();
	}

	private void DrawModelList(ObjectInfo info) {
		var dim = Helpers.DimColor(ImGuiCol.Text, 0.80f);
		foreach (var mdl in info.Models) {
			ImGui.TableNextRow();
            
			ImGui.TableSetColumnIndex(1);
			ImGui.Text(mdl.GetSlotString());

			ImGui.TableSetColumnIndex(2);
			var indent = ImGui.GetColumnWidth() * 0.065f;
			ImGui.Indent(indent);
			DrawPath(mdl.Path);
			ImGui.Unindent(indent);
		}
		if (dim) ImGui.PopStyleColor(1);
	}

	private bool DrawColumnSelect(string content, bool selected = false) {
		var style = ImGui.GetStyle();
		var spacing = ImGui.GetItemRectSize().Y - (style.ItemSpacing.Y + style.ItemInnerSpacing.Y) * 2;
		ImGui.PushStyleVar(
			ImGuiStyleVar.ItemSpacing,
			ImGui.GetStyle().ItemSpacing with { Y = spacing }
		);
		
		try {
			ImGui.SetCursorPosY(ImGui.GetCursorPosY() + spacing / 2);
			return ImGui.Selectable(content, selected);
		} finally {
			ImGui.PopStyleVar();
		}
	}

	private void DrawPath(string path) {
		if (!DrawColumnSelect(path)) return;
		ImGui.SetClipboardText(path);
		this._chat.PrintMessage($"Model path copied to clipboard:\n{path}");
	}

	private void SortTable(ImGuiTableColumnSortSpecsPtr sort, List<ObjectInfo> list) {
		var sortDir = sort.SortDirection == ImGuiSortDirection.Ascending ? -1 : 1;
		list.Sort((a, b) => {
			return sort.ColumnIndex switch {
				0 => a.Distance < b.Distance ? sortDir : -sortDir,
				1 => string.Compare(
						a.GetItemTypeString(),
						b.GetItemTypeString(),
						StringComparison.Ordinal
					) * sortDir,
				2 => string.Compare(
						string.Join(' ', a.Models.Select(mdl => mdl.Path)),
						string.Join(' ', b.Models.Select(mdl => mdl.Path)),
						StringComparison.Ordinal
					) * sortDir,
				_ => 0
			};
		});
	}
	
	// Popups

	private void DrawPopups(ConfigFile config) {
		ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, ImGui.GetStyle().WindowRounding);
		try {
			DrawSettings(config);
			DrawFilters(config);
		} finally {
			ImGui.PopStyleVar();
		}
	}
	
	// Settings

	private void DrawSettings(ConfigFile config) {
		if (!ImGui.BeginPopup(SettingsPopupId)) return;

		DrawCircleSettings("Outer circle display (Max range)", ref config.Overlay.Max);
		ImGui.Spacing();
		DrawCircleSettings("Inner circle display (Min range)", ref config.Overlay.Min);
		
		ImGui.EndPopup();
	}

	private void DrawCircleSettings(string text, ref OverlayElement data) {
		ImGui.Text(text);

		var col4 = ImGui.ColorConvertU32ToFloat4(data.Color);
		if (ImGui.ColorEdit4($"Color##{text}", ref col4))
			data.Color = ImGui.ColorConvertFloat4ToU32(col4);
		
		ImGui.SliderFloat($"Width##{text}", ref data.Width, 1.0f, 10.0f);
	}
	
	// Filters
	
	private void DrawFilters(ConfigFile config) {
		if (!ImGui.BeginPopup(FilterPopupId)) return;

		var filter = config.Filters;
		var chara = filter.Flags.HasFlag(ObjectFilterFlags.Chara);

		ImGui.BeginTable("ObjectTypeTable", 2, ImGuiTableFlags.NoSavedSettings);
		ImGui.TableSetupColumn("Object Types");
		ImGui.TableSetupColumn("Character Models", chara ? ImGuiTableColumnFlags.None : ImGuiTableColumnFlags.Disabled);
		
		ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, 0);
		ImGui.TableHeadersRow();
		ImGui.PopStyleColor();
		ImGui.TableNextRow();

		ImGui.TableSetColumnIndex(0);
		DrawFilterFlag(filter, "BgObject", ObjectFilterFlags.BgObject);
        DrawFilterFlag(filter, "Character", ObjectFilterFlags.Chara);
		DrawFilterFlag(filter, "Terrain", ObjectFilterFlags.Terrain);

		if (chara) {
			ImGui.TableSetColumnIndex(1);
			DrawFilterFlag(filter, "Human", ObjectFilterFlags.Human);
			DrawFilterFlag(filter, "DemiHuman", ObjectFilterFlags.DemiHuman);
			DrawFilterFlag(filter, "Monster", ObjectFilterFlags.Monster);
			DrawFilterFlag(filter, "Weapon", ObjectFilterFlags.Weapon);
		}

		ImGui.EndTable();
		ImGui.EndPopup();
	}

	private void DrawFilterFlag(ObjectFilters filter, string label, ObjectFilterFlags flag) {
		var value = filter.Flags.HasFlag(flag);
		if (ImGui.Checkbox(label, ref value))
			filter.Flags ^= flag;
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
