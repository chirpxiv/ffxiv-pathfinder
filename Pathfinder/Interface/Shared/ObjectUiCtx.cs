using Dalamud.Interface;

using Pathfinder.Objects.Data;
using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Interface.Shared; 

[LocalService]
public class ObjectUiCtx {
	private readonly UiBuilder _ui;
	
	public ObjectUiCtx(UiBuilder _ui) {
		this._ui = _ui;
	}
	
	// Hover ctx
    
	public int SetterId;
	private ulong LastUpdateFrame;
	
	private ObjectInfo? __hover;
	public ObjectInfo? Hovered {
		get => this.__hover != null && this.LastUpdateFrame >= this._ui.FrameCount - 1 ? this.__hover : null;
		private set {
			this.LastUpdateFrame = this._ui.FrameCount;
			this.__hover = value;
		}
	}

	public void SetHovered(ObjectInfo? value, int id = -1) {
		this.Hovered = value;
		this.SetterId = id;
	}
	
	public bool HoveredThisFrame => this.LastUpdateFrame == this._ui.FrameCount;
}
