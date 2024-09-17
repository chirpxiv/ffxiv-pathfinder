using System;
using System.Linq;
using System.Collections.Generic;

using Dalamud.Plugin.Services;

using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

using Pathfinder.Events;
using Pathfinder.Objects.Data;
using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Objects;

public delegate void ObjectsUpdatedHandler(ObjectWatcher sender, IEnumerable<ObjectInfo> objects);

[GlobalService]
public class ObjectWatcher : IDisposable {
	private readonly IFramework _framework;
	
	public ObjectWatcher(IFramework framework, InitEvent init) {
		this._framework = framework;
		init.Subscribe(this.OnInit);
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

		var objects = this.RecurseWorld()
			.Where(obj => obj.ObjectType is ObjectType.BgObject or ObjectType.Terrain or ObjectType.CharacterBase)
			.Select(obj => obj.GetObjectInfo());
		
		this.OnObjectsUpdated?.Invoke(this, objects);
	}

	private unsafe WorldObject? GetWorld() {
		var world = World.Instance();
		return world != null ? new WorldObject(&world->Object) : null;
	}

	private IEnumerable<WorldObject> RecurseWorld() {
		var worldObj = this.GetWorld();
		if (worldObj == null) yield break;

		yield return worldObj;
		
		foreach (var sibling in worldObj.GetSiblings()) {
			yield return sibling;
			foreach (var child in this.RecurseChildren(sibling))
				yield return child;
		}

		foreach (var child in this.RecurseChildren(worldObj))
			yield return child;
	}

	private IEnumerable<WorldObject> RecurseChildren(WorldObject worldObj) {
		foreach (var child in worldObj.GetChildren()) {
			yield return child;
			foreach (var reChild in this.RecurseChildren(child))
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
		
		this._framework.Update -= this.OnUpdate;
		this.OnObjectsUpdated = null;
		
		this._clients.Clear();
	}
}
