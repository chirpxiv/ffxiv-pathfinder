using System;

using Dalamud.Logging;
using Dalamud.Plugin;

using Microsoft.Extensions.DependencyInjection;

using ObjectFinder.Services;

namespace ObjectFinder;

public sealed class ObjectFinder : IDalamudPlugin {
	// Plugin information
	
	public string Name => "Object Finder";

	// Services

	private readonly ServiceProvider _services;
	
	// Ctor & Initialization
	
	public ObjectFinder(DalamudPluginInterface api) {
		try {
			this._services = new ServiceFactory()
				.AddDalamud(api)
				.CreateProvider();
		} catch (Exception err) {
			PluginLog.Error($"Failed to initialize plugin:\n{err}");
			this.Dispose();
			throw;
		}
	}

	public void Dispose() => this._services.Dispose();
}
