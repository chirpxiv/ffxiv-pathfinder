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

namespace Pathfinder.Interface.Windows; 

[LocalService]
public class OverlayWindow : Window, IDisposable {
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
		objectInfo.ForEach(info => DrawObjectDot(drawList, dot, info));
	}

	private void DrawObjectDot(ImDrawListPtr drawList, OverlayDotElement dot, ObjectInfo info) {
		if (!this._gui.WorldToScreen(info.Position, out var point))
			return;

		drawList.AddCircleFilled(point, dot.Radius + dot.Width - 1.0f, dot.Color);
		if (dot.Width > 0.0f)
			drawList.AddCircle(point, dot.Radius + dot.Width / 2, dot.OutlineColor, 16, dot.Width);
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
