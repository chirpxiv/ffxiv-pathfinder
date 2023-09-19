using System;

using Dalamud.Plugin;
using Dalamud.Logging;

using Microsoft.Extensions.DependencyInjection;

using Pathfinder.Config;
using Pathfinder.Events;
using Pathfinder.Services.Core;

namespace Pathfinder;

public sealed class Pathfinder : IDalamudPlugin {
	// Plugin information
	
	public string Name => "Object Finder";

	// Services

	private readonly ServiceProvider _services;
	
	// Ctor & Initialization
	
	public Pathfinder(DalamudPluginInterface api) {
		try {
			using var factory = new ServiceFactory();
			
			this._services = factory
				.AddDalamud(api)
				.ResolveAll()
				.CreateProvider();
				
			factory.Initialize(this._services);

			this._services.GetRequiredService<ConfigService>().Load();

			using var _initEvent = this._services.GetRequiredService<InitEvent>();
			_initEvent.Invoke();

		} catch {
			Dispose();
			throw;
		}
	}

	public void Dispose() {
		try {
			this._services.Dispose();
		} catch (Exception err) {
			PluginLog.Error($"Failed to dispose:\n{err}");
		}
	}
}
