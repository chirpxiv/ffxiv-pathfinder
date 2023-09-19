using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using Dalamud.Game;
using Dalamud.Logging;

using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

using ObjectFinder.Events;
using ObjectFinder.Objects.Data;
using ObjectFinder.Interop.Structs;
using ObjectFinder.Services.Core.Attributes;

namespace ObjectFinder.Objects;

public delegate void ObjectsUpdatedHandler(ObjectWatcher sender, IEnumerable<ObjectInfo> objects);

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
	
	// Events

	public ObjectsUpdatedHandler? OnObjectsUpdated;
	
	// State

	private bool __enabled;

	public bool IsEnabled {
		get => this.__enabled && !this.IsDisposed;
		private set => this.__enabled = value;
	}
	
	// Update handler

	private void OnUpdate(object _sender) {
		if (!this.IsEnabled) return;

		var objects = RecurseWorld()
			.Where(obj => obj.ObjectType is ObjectType.BgObject or ObjectType.Terrain or ObjectType.CharacterBase)
			.Select(obj => obj.GetObjectInfo());
		
		this.OnObjectsUpdated?.Invoke(this, objects);
	}

	private unsafe WorldObject? GetWorld() {
		var world = World.Instance();
		return world != null ? new WorldObject(&world->Object) : null;
	}

	private IEnumerable<WorldObject> RecurseWorld() {
		var worldObj = GetWorld();
		if (worldObj == null) yield break;

		yield return worldObj;
		
		foreach (var sibling in worldObj.GetSiblings()) {
			yield return sibling;
			foreach (var child in RecurseChildren(sibling))
				yield return child;
		}

		foreach (var child in RecurseChildren(worldObj))
			yield return child;
	}

	private IEnumerable<WorldObject> RecurseChildren(WorldObject worldObj) {
		foreach (var child in worldObj.GetChildren()) {
			yield return child;
			foreach (var reChild in RecurseChildren(child))
				yield return reChild;
		}
	}
	
	// IObservable

	private readonly List<IObjectClient> _clients = new();

	private bool HasClients => this._clients.Count > 0;

	public void AddClient(IObjectClient client) {
		this._clients.Add(client);
		this.IsEnabled |= this.HasClients;
	}

	public void Remove(IObjectClient client) {
		this._clients.Remove(client);
		this.IsEnabled &= this.HasClients;
	}
	
	// Disposal

	private bool IsDisposed;

	public void Dispose() {
		if (this.IsDisposed) return;
		
		this.IsDisposed = true;
		this.IsEnabled = false;
		
		this._framework.Update -= OnUpdate;
		this.OnObjectsUpdated = null;
		
		this._clients.Clear();
	}
}
