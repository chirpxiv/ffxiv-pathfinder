using System;
using System.Linq;
using System.Collections.Generic;

using ImGuiNET;

using Pathfinder.Config;
using Pathfinder.Objects.Data;
using Pathfinder.Interface.Widgets;
using Pathfinder.Services.Core.Attributes;
using Pathfinder.Services;

namespace Pathfinder.Interface.Components;

[LocalService(ServiceFlags.Transient)]
public class ResultsTable {
	private enum Column : int {
		Distance = 0,
		Type = 1,
		Address = 2,
		Paths = 3
	}
	
	// Constructor

	private readonly ChatService _chat;
	
	public ResultsTable(ChatService _chat) {
		this._chat = _chat;
	}
	
	// Column wrappers

	private void SetColumnIndex(Column column) => ImGui.TableSetColumnIndex((int)column);
	
	// UI Draw
	
	public void Draw(IObjectClient client, ConfigFile config, uint id = 0x0B75) {
		var objects = client.GetObjects().ToList();

		const ImGuiTableFlags TableFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable | ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.ScrollX;
		ImGui.BeginChildFrame(id, ImGui.GetContentRegionAvail());
		ImGui.BeginTable("##ObjectSearchTable", 4, TableFlags);
		
		var avail = ImGui.GetContentRegionAvail().X;
		ImGui.TableSetupColumn("Distance", ImGuiTableColumnFlags.DefaultSort, avail * 0.125f);
		ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.None, avail * 0.175f);
		ImGui.TableSetupColumn("Address", config.Table.ShowAddress ? ImGuiTableColumnFlags.None : ImGuiTableColumnFlags.Disabled, avail * 0.25f);
		ImGui.TableSetupColumn("Paths", ImGuiTableColumnFlags.None, avail * 0.7f);
        ImGui.TableHeadersRow();
		
		SortTable(ImGui.TableGetSortSpecs().Specs, objects);
		objects.ForEach(item => DrawObjectEntry(config, item));
		
		ImGui.EndTable();
		
		ImGui.EndChildFrame();
	}

	private void DrawObjectEntry(ConfigFile config, ObjectInfo info) {
		var showAddress = config.Table.ShowAddress;
		var useColors = config.Table.UseColors;
		
		ImGui.TableNextRow();

		if (useColors) {
			var color = config.GetColor(info.FilterType);
			ImGui.PushStyleColor(ImGuiCol.Text, color);
		}

		try {
			SetColumnIndex(Column.Distance);
			ImGui.Text(info.Distance.ToString("0.00"));

			SetColumnIndex(Column.Type);
			ImGui.Text(info.GetItemTypeString());

			if (showAddress) {
				SetColumnIndex(Column.Address);
				DrawAddress(info.Address);
			}

			SetColumnIndex(Column.Paths);
			DrawObjectPaths(info, showAddress);
		} finally {
			if (useColors) ImGui.PopStyleColor();
		}
	}

	private void DrawObjectPaths(ObjectInfo info, bool showAddress = false) {
		var count = info.Models.Count;
		if (count > 1) {
			ImGui.PushID($"Object_{info.Address:X}");

			var imKey = ImGui.GetID("Expand");
			var state = ImGui.GetStateStorage();
			var isExpand = state.GetBool(imKey);
				
			if (DrawColumnSelect($"{count} entries (Click to {(isExpand ? "collapse" : "expand")})", isExpand)) {
				isExpand = !isExpand;
				state.SetBool(imKey, isExpand);
			}
				
			if (isExpand) DrawModelList(info, showAddress);
		} else {
			var text = info.Models.FirstOrDefault()?.Path ?? string.Empty;
			DrawPath(text);
		}
	}

	private void DrawModelList(ObjectInfo info, bool showAddress = false) {
		var dim = Helpers.DimColor(ImGuiCol.Text, 0.80f);
		foreach (var mdl in info.Models) {
			ImGui.TableNextRow();
			
			SetColumnIndex(Column.Type);
			ImGui.Text(mdl.GetSlotString());

			if (showAddress) {
				SetColumnIndex(Column.Address);
				DrawAddress(mdl.Address);
			}

			SetColumnIndex(Column.Paths);
			var indent = ImGui.GetColumnWidth() * 0.065f;
			ImGui.Indent(indent);
			DrawPath(mdl.Path);
			ImGui.Unindent(indent);
		}
		if (dim) ImGui.PopStyleColor(1);
	}

	private void DrawAddress(nint addr) {
		var text = addr.ToString("X");
		if (!DrawColumnSelect(text)) return;
		ImGui.SetClipboardText(text);
		this._chat.PrintMessage($"Address copied to clipboard: {text}");
	}

	private void DrawPath(string path) {
		if (!DrawColumnSelect(path)) return;
		ImGui.SetClipboardText(path);
		this._chat.PrintMessage($"Model path copied to clipboard:\n{path}");
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

	private void SortTable(ImGuiTableColumnSortSpecsPtr sort, List<ObjectInfo> list) {
		var sortDir = sort.SortDirection == ImGuiSortDirection.Ascending ? -1 : 1;
		list.Sort((a, b) => {
			return sort.ColumnIndex switch {
				0 => a.Distance.CompareTo(b.Distance) * sortDir,
				1 => a.Address.CompareTo(b.Address) * sortDir,
				2 => string.Compare(
						a.GetItemTypeString(),
						b.GetItemTypeString(),
						StringComparison.Ordinal
					) * sortDir,
				3 => string.Compare(
						a.Models.FirstOrDefault()?.Path,
						b.Models.FirstOrDefault()?.Path,
						StringComparison.Ordinal
					) * sortDir,
				_ => 0
			};
		});
	}
}
