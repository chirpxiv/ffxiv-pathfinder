using System;
using System.Linq;
using System.Numerics;

using ImGuiNET;

using Dalamud.Plugin.Services;
using Dalamud.Interface.Windowing;

using Pathfinder.Config;
using Pathfinder.Config.Data;
using Pathfinder.Objects;
using Pathfinder.Objects.Data;
using Pathfinder.Services;
using Pathfinder.Services.Core.Attributes;
using Pathfinder.Interface.Components;
using Pathfinder.Interface.Shared;
using Pathfinder.Interface.Widgets;

namespace Pathfinder.Interface.Windows; 

[LocalService]
public class OverlayWindow : Window, IDisposable {
	private readonly ObjectUiCtx _ctx;
	
	private readonly ConfigService _config;
	private readonly ObjectService _objects;
	private readonly PerceptionService _wis;
	
	private readonly MainWindow _mainWin;
	private readonly ImCircle3D _circle3d;
	
	private readonly IGameGui _gui;

	private IObjectClient? _client;

	private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground
		| ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBringToFrontOnFocus;
	
	public OverlayWindow(
		ObjectUiCtx _ctx,
		ConfigService _config,
		ObjectService _objects,
		PerceptionService _wis,
		MainWindow _mainWin,
		ImCircle3D _circle3d,
		IGameGui _gui
	) : base(
		"##PathfinderOverlay",
		WindowFlags
	) {
		this._ctx = _ctx;
		
		this._config = _config;
		this._objects = _objects;
		this._wis = _wis;
		
		this._mainWin = _mainWin;
		this._circle3d = _circle3d;
		
		this._gui = _gui;
	}
	
	// Client

	private IObjectClient GetClient()
		=> this._client ??= this._objects.CreateClient();
	
	// UI Draw

	public override void PreOpenCheck() => this.IsOpen = this._mainWin.IsOpen;

	public override void PreDraw() {
		Size = ImGui.GetIO().DisplaySize;
		Position = Vector2.Zero;
	}

	public override void Draw() {
		var config = this._config.Get();
		if (!config.Overlay.Enabled) return;

		var pos = this._wis.GetPosition();
		var drawList = ImGui.GetBackgroundDrawList();
		
		if (config.Overlay.HoverLine.Draw && this._ctx.Hovered != null)
			DrawLineTo(drawList, config, pos, this._ctx.Hovered);
		
		DrawRadiusCircles(config, drawList, pos);
		DrawObjectDots(config, drawList);
	}
	
	// Radius circle

	private void DrawRadiusCircles(ConfigFile config, ImDrawListPtr drawList, Vector3 centerPos) {
		var minRadius = Math.Min(config.Filters.MinRadius.Value, config.Filters.MaxRadius.Value);
		var maxRadius = Math.Max(minRadius, config.Filters.MaxRadius.Value);
		if (config.Filters.MinRadius.Enabled)
			this._circle3d.Draw(drawList, centerPos, minRadius, config.Overlay.Min);
		if (config.Filters.MaxRadius.Enabled)
			this._circle3d.Draw(drawList, centerPos, maxRadius, config.Overlay.Max);
	}
	
	// Item dots

	private void DrawObjectDots(ConfigFile config, ImDrawListPtr drawList) {
		var dot = config.Overlay.ItemDot;
		if (!dot.Draw) return;
		
		var objectInfo = GetClient().GetObjects().ToList();
		objectInfo.ForEach(info => DrawObjectDot(drawList, config, dot, info));
	}

	private void DrawObjectDot(ImDrawListPtr drawList, ConfigFile config, OverlayDotElement dot, ObjectInfo info) {
		if (!this._gui.WorldToScreen(info.Position, out var point))
			return;

		var color = dot.ColorOverride ? dot.Color : config.GetColor(info.FilterType);
		var outlineColor = dot.OutlineColor;
		if (config.Overlay.ItemDot.DimOnHover && this._ctx.Hovered != null && this._ctx.Hovered != info) {
			color = Helpers.ColorAlpha(color, 0x70);
			outlineColor = Helpers.ColorAlpha(outlineColor, 0x70);
		}

		drawList.AddCircleFilled(point, dot.Radius + dot.Width - 1.0f, color);
		if (dot.Width > 0.0f)
			drawList.AddCircle(point, dot.Radius + dot.Width / 2, outlineColor, 16, dot.Width);
	}
	
	// Hover line

	private void DrawLineTo(ImDrawListPtr drawList, ConfigFile config, Vector3 centerPos, ObjectInfo info) {
		if (!this._gui.WorldToScreen(centerPos, out var centerPos2d))
			return;

		this._gui.WorldToScreen(info.Position, out var objectPos2d);

		var line = config.Overlay.HoverLine;
		var color = line.ColorOverride ? line.Color : config.GetColor(info.FilterType);
		drawList.AddLine(centerPos2d, objectPos2d, color, line.Width);
	}
	
	// Window close

	public override void OnClose() {
		Dispose();
	}
	
	// Disposal

	public void Dispose() {
		this._client?.Dispose();
	}
}
