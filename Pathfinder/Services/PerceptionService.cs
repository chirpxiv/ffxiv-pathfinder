using System.Numerics;

using Dalamud.Interface;
using Dalamud.Plugin.Services;
using CameraManager = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager;

using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Services; 

[GlobalService]
public class PerceptionService {
	private readonly IClientState _state;
	private readonly UiBuilder _ui;
	
	public PerceptionService(IClientState _state, UiBuilder _ui) {
		this._state = _state;
		this._ui = _ui;
	}
	
	// Retrieve position based on either player or otherwise camera.

	public Vector3 GetPosition()
		=> GetPlayerPosition() ?? GetCameraPosition() ?? Vector3.Zero;

	public Vector2 GetPosition2D() {
		var pos = GetPosition();
		return new Vector2(pos.X, pos.Z);
	}

	private bool IsPlayerActive()
		=> this._state.IsLoggedIn && !(this._ui.CutsceneActive || this._ui.GposeActive);

	private Vector3? GetPlayerPosition()
		=> IsPlayerActive() ? this._state.LocalPlayer?.Position : null;

	private unsafe Vector3? GetCameraPosition() {
		var manager = CameraManager.Instance();
		var camera = manager != null ? manager->CurrentCamera : null;
		return camera != null ? camera->Object.Position : null;
	}
}
