using System;
using System.Linq;
using System.Numerics;

using ImGuiNET;

using Dalamud.Plugin.Services;
using Dalamud.Interface.Windowing;

using Pathfinder.Config;
using Pathfinder.Objects;
using Pathfinder.Objects.Data;
using Pathfinder.Services;
using Pathfinder.Services.Core.Attributes;
using Pathfinder.Interface.Components;

namespace Pathfinder.Interface.Windows; 

[LocalService]
public class OverlayWindow : Window, IDisposable {
	private readonly MainWindow _mainWin;
	private readonly ImCircle3D _circle3d;

	private readonly ConfigService _config;
	private readonly ObjectService _objects;
	private readonly PerceptionService _wis;
	
	private readonly IGameGui _gui;

	private IObjectClient? _client;

	private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground
		| ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBringToFrontOnFocus;
	
	public OverlayWindow(
		MainWindow _mainWin,
		ImCircle3D _circle3d,
		ConfigService _config,
		ObjectService _objects,
		PerceptionService _wis,
		IGameGui _gui
	) : base(
		"##PathfinderOverlay",
		WindowFlags
	) {
		this._mainWin = _mainWin;
		this._circle3d = _circle3d;

		this._config = _config;
		this._objects = _objects;
		this._wis = _wis;
		
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
		DrawObjectPaths(config, drawList, pos);
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
	
	// Object path render

	private void DrawObjectPaths(ConfigFile config, ImDrawListPtr drawList, Vector3 centerPos) {
		var objectInfo = GetClient().GetObjects().ToList();
		foreach (var info in objectInfo) {
			if (!this._gui.WorldToScreen(info.Position, out var point)) continue;

			drawList.AddCircleFilled(point, 5f, 0xFFFFFFFF);

			point.X += 5;
			ImGui.SetCursorScreenPos(point);
			ImGui.Text(string.Join("\n", info.Models.Select(mdl => mdl.Path)));
		}
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
