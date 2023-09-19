using System;
using System.Linq;
using System.Collections.Generic;

using Dalamud.Game;
using Dalamud.Logging;

using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

using ObjectFinder.Events;
using ObjectFinder.Objects.Data;
using ObjectFinder.Interop.Structs;
using ObjectFinder.Services.Core.Attributes;

namespace ObjectFinder.Objects; 

[GlobalService]
public class ObjectWatcher : IDisposable {
	private readonly Framework _framework;
	
	public ObjectWatcher(Framework _framework, InitEvent _init) {
		this._framework = _framework;
		_init.Subscribe(OnInit);
	}

	private void OnInit() {
		this._framework.Update += OnUpdate;
	}
	
	// State

	private bool __enabled;

	public bool IsEnabled {
		get => this.__enabled && !this.IsDisposed;
		private set => this.__enabled = value;
	}

	public void Start() => this.IsEnabled = true;

	public void Stop() => this.IsEnabled = false;
	
	// Objects

	private readonly List<ObjectInfo> _objects = new();

	public IReadOnlyList<ObjectInfo> GetObjects() {
		lock (this._objects) {
			return this._objects.AsReadOnly();
		}
	}
	
	// Update handler

	private unsafe void OnUpdate(object _sender) {
		if (!this.IsEnabled) return;
        
		var world = World.Instance();
		if (world == null) return;

		var worldObj = new WorldObject(&world->Object);

		var objects = RecurseObjects(worldObj)
			.Where(obj => obj.ObjectType is ObjectType.BgObject or ObjectType.Terrain or ObjectType.CharacterBase)
			.Select(obj => obj.GetObjectInfo());
		
		lock (this._objects) {
			this._objects.Clear();
			this._objects.AddRange(objects);
			PluginLog.Information($"Added {this._objects.Count} objects");
		}
	}

	private IEnumerable<WorldObject> RecurseObjects(WorldObject worldObj) {
		foreach (var child in worldObj.GetChildren()) {
			yield return child;
			foreach (var reChild in RecurseObjects(child))
				yield return reChild;
		}
	}
	
	// Disposal

	private bool IsDisposed;

	public void Dispose() {
		if (this.IsDisposed) return;
		this.IsDisposed = true;
		this.IsEnabled = false;
		this._framework.Update -= OnUpdate;
	}
}
