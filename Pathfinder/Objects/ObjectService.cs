using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

using Pathfinder.Config;
using Pathfinder.Events;
using Pathfinder.Objects.Data;
using Pathfinder.Services;
using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Objects;

[GlobalService]
public class ObjectService : IDisposable {
	private readonly ObjectWatcher _watcher;

	private readonly ConfigService _config;
	private readonly PerceptionService _wis;
	
	public ObjectService(ObjectWatcher _watcher, ConfigService _config, PerceptionService _wis, InitEvent _init) {
		this._watcher = _watcher;

		this._config = _config;
		this._wis = _wis;
		
		_init.Subscribe(OnInit);
	}

	private void OnInit() {
		this._watcher.OnObjectsUpdated += OnObjectsUpdated;
	}
	
	// Events

	private readonly List<ObjectInfo> _objects = new();

	private IEnumerable<ObjectInfo> GetObjects() {
		lock (this._objects) {
			return this._objects.AsReadOnly();
		}
	}

	private void OnObjectsUpdated(object _sender, IEnumerable<ObjectInfo> objects) {
		lock (this._objects) {
			this._objects.Clear();
			this._objects.AddRange(ApplyFilter(objects));
		}
	}

	private IEnumerable<ObjectInfo> ApplyFilter(IEnumerable<ObjectInfo> objects) {
		var pos = this._wis.GetPosition2D();

		var filter = this._config.Get().Filters;
		var min = filter.MinRadius;
		var max = filter.MaxRadius;
		
		return objects.Where(worldObj => {
			var result = true;
			
			// Object flags
			if (!filter.Flags.HasFlag(worldObj.FilterFlags))
				return false;
			
			// Distance
			var objPos = new Vector2(worldObj.Position.X, worldObj.Position.Z);
			var dist = Vector2.Distance(pos, objPos);
			if (min.Enabled) result &= dist >= min.Value;
			if (max.Enabled) result &= dist <= max.Value;
			if (result) worldObj.Distance = dist;
			
			// Search string
			if (result && filter.SearchString != string.Empty)
				result &= worldObj.Models.Any(data => data.Path.Contains(filter.SearchString, StringComparison.OrdinalIgnoreCase));
			
			return result;
		});
	}
	
	// Client

	public IObjectClient CreateClient() {
		var client = new ObjectClient(this, this._watcher);
		this._watcher.AddClient(client);
		return client;
	}

	private class ObjectClient : IObjectClient {
		private readonly ObjectService _objects;
		private readonly ObjectWatcher _watcher;

		public ObjectClient(ObjectService _objects, ObjectWatcher _watcher) {
			this._objects = _objects;
			this._watcher = _watcher;
		}

		public IEnumerable<ObjectInfo> GetObjects() => this._objects.GetObjects();

		public void Dispose() => this._watcher.Remove(this);
	}
	
	// Disposal

	public void Dispose() {
		this._watcher.OnObjectsUpdated -= OnObjectsUpdated;
	}
}
