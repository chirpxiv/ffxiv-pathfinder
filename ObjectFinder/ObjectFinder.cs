using System;

using Dalamud.Logging;
using Dalamud.Plugin;

using Microsoft.Extensions.DependencyInjection;

using ObjectFinder.Interface;
using ObjectFinder.Services;
using ObjectFinder.Services.Attributes;

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
				.AddResolveType<GlobalServiceAttribute>()
				.CreateProvider();

			this._services.GetRequiredService<PluginGui>();
		} catch (Exception err) {
			PluginLog.Error($"Failed to initialize plugin:\n{err}");
			this.Dispose();
			throw;
		}
	}

	public void Dispose() => this._services.Dispose();
}
