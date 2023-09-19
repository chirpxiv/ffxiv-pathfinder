using System;

using ObjectFinder.Services.Core.Attributes;

namespace ObjectFinder.Objects;

[LocalService]
public class ObjectService : IDisposable {
	private readonly ObjectWatcher _watcher;
	
	public ObjectService(ObjectWatcher _watcher) {
		this._watcher = _watcher;
	}
	
	// Disposal

	public void Dispose() {
		
	}
}
