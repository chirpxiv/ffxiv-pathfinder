using System.Numerics;

using Dalamud.Interface;
using Dalamud.Plugin.Services;
using CameraManager = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager;

using ImGuiNET;

using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Services; 

[GlobalService]
public class PerceptionService {
	private readonly IClientState _state;
	private readonly ChatService _chat;
	private readonly UiBuilder _ui;
	
	public PerceptionService(IClientState _state, ChatService _chat, UiBuilder _ui) {
		this._state = _state;
		this._chat = _chat;
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
	
	// Copy data to clipboard

	public void SetClipboardAddress(nint address)
		=> SetClipboard("Address copied to clipboard:", address.ToString("X"));

	public void SetClipboardPath(string path)
		=> SetClipboard("Model path copied to clipboard:", path, "\n");

	private void SetClipboard(string? msg, string content, string delim = " ") {
		ImGui.SetClipboardText(content);
		if (msg != null)
			this._chat.PrintMessage($"{msg}{delim}{content}");
	}
}
