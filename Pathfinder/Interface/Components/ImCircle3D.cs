using System;
using System.Numerics;

using Dalamud.Plugin.Services;

using ImGuiNET;

using Pathfinder.Config.Data;
using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Interface.Components; 

[LocalService(ServiceFlags.Transient)]
public class ImCircle3D {
	// Global allocated buffer of 2D points passed to ImGui.
	
	private const int PointCount = 361;
	private readonly static Vector2[] Points = new Vector2[PointCount];
	
	// Constructor

	private readonly IGameGui _gui;

	public ImCircle3D(IGameGui _gui) {
		this._gui = _gui;
	}
	
	// Draw

	public void Draw(ImDrawListPtr drawList, Vector3 centerPos, float radius, OverlayElement data) {
		if (!data.Draw) return;
		Draw(drawList, centerPos, radius, data.Color, data.Width);
	}

	public void Draw(ImDrawListPtr drawList, Vector3 centerPos, float radius, uint color = 0xFFFFFFFF, float thickness = 2f) {
		var start = 0;
		for (var n = 0; n < PointCount; n++) {
			var x = radius * (float)Math.Sin((Math.PI * 2.0f) * (n / 360.0f)) + centerPos.X;
			var z = radius * (float)Math.Cos((Math.PI * 2.0f) * (n / 360.0f)) + centerPos.Z;
			var vis = this._gui.WorldToScreen(centerPos with { X = x, Z = z }, out Points[n]);
			if (vis && n != PointCount - 1) continue;
			
			var ct = n - start + (vis ? 1 : 0);
			drawList.AddPolyline(ref Points[start], ct, color, ImDrawFlags.RoundCornersAll, thickness);
			start = n;
		}
	}
}
