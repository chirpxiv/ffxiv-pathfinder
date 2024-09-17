using System.Collections.Generic;
using System.Linq;
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
	private readonly IUiBuilder _ui;
	
	public PerceptionService(IClientState state, ChatService chat, IUiBuilder ui) {
		this._state = state;
		this._chat = chat;
		this._ui = ui;
	}
	
	// Retrieve position based on either player or otherwise camera.

	public Vector3 GetPosition() => this.GetPlayerPosition() ?? this.GetCameraPosition() ?? Vector3.Zero;

	public Vector2 GetPosition2D() {
		var pos = this.GetPosition();
		return new Vector2(pos.X, pos.Z);
	}

	private bool IsPlayerActive() => this._state.IsLoggedIn && !(this._ui.CutsceneActive || this._state.IsGPosing);

	private Vector3? GetPlayerPosition() => this.IsPlayerActive() ? this._state.LocalPlayer?.Position : null;

	private unsafe Vector3? GetCameraPosition() {
		var manager = CameraManager.Instance();
		var camera = manager != null ? manager->CurrentCamera : null;
		return camera != null ? camera->Object.Position : null;
	}
	
	// Copy data to clipboard

	public void SetClipboardAddress(nint address) => this.SetClipboard("Address copied to clipboard:", address.ToString("X"));

	public void SetClipboardPath(string path) => this.SetClipboard("Model path copied to clipboard:", path, "\n");

	private void SetClipboard(string? msg, string content, string delim = " ") {
		ImGui.SetClipboardText(content);
		if (msg != null)
			this._chat.PrintMessage($"{msg}{delim}{content}");
	}
	
	public void SetClipboardPaths(List<string> paths) {
		switch (paths.Count) {
			case 1:
				this.SetClipboardPath(paths.First());
				return;
			case > 0:
				var content = string.Join("\n", paths);
				ImGui.SetClipboardText(content);
				break;
		}

		this._chat.PrintMessage($"Copied {paths.Count} paths to clipboard.");
	}
}
