using System;
using System.Diagnostics;

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
			using var factory = new ServiceFactory();
			
			this._services = factory
				.AddDalamud(api)
				.ResolveAll()
				.CreateProvider();
				
			factory.Initialize(this._services);
		} catch (Exception err) {
			PluginLog.Error($"Failed to initialize plugin:\n{err}");
			this.Dispose();
			throw;
		}
	}
	
	public void Dispose() => this._services.Dispose();
}
