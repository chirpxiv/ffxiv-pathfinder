using System;
using System.Collections.Generic;

using ObjectFinder.Events;
using ObjectFinder.Objects.Data;
using ObjectFinder.Services.Core.Attributes;

namespace ObjectFinder.Objects;

[GlobalService]
public class ObjectService : IDisposable {
	private readonly ObjectWatcher _watcher;
	
	public ObjectService(ObjectWatcher _watcher, InitEvent _init) {
		this._watcher = _watcher;
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
			this._objects.AddRange(objects);
		}
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
