using Dalamud.Interface;

using Pathfinder.Objects.Data;
using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Interface.Shared; 

[LocalService]
public class ObjectUiCtx {
	private readonly IUiBuilder _ui;
	
	public ObjectUiCtx(IUiBuilder ui) {
		this._ui = ui;
	}
	
	// Hover ctx
    
	public int SetterId;
	private ulong LastUpdateFrame;
	
	private ObjectInfo? _hover;
	public ObjectInfo? Hovered {
		get => this._hover != null && this.LastUpdateFrame >= this._ui.FrameCount - 1 ? this._hover : null;
		private set {
			this.LastUpdateFrame = this._ui.FrameCount;
			this._hover = value;
		}
	}

	public void SetHovered(ObjectInfo? value, int id = -1) {
		this.Hovered = value;
		this.SetterId = id;
	}
	
	public bool HoveredThisFrame => this.LastUpdateFrame == this._ui.FrameCount;
}
