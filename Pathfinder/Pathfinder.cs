using System;

using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using Microsoft.Extensions.DependencyInjection;

using Pathfinder.Config;
using Pathfinder.Events;
using Pathfinder.Services.Core;

namespace Pathfinder;

public sealed class Pathfinder : IDalamudPlugin {
	// Plugin information
	
	public string Name => "Pathfinder";

	// Services

	private readonly IPluginLog _log;
	private readonly ServiceProvider _services;
	
	// Ctor & Initialization
	
	public Pathfinder(
		IDalamudPluginInterface api,
		IPluginLog log
	) {
		this._log = log;
		
		try {
			using var factory = new ServiceFactory(log);
			
			this._services = factory
				.AddDalamud(api)
				.ResolveAll()
				.CreateProvider();
				
			factory.Initialize(this._services);

			this._services.GetRequiredService<ConfigService>().Load();

			using var initEvent = this._services.GetRequiredService<InitEvent>();
			initEvent.Invoke();

		} catch {
			this.Dispose();
			throw;
		}
	}

	public void Dispose() {
		try {
			this._services.Dispose();
		} catch (Exception err) {
			this._log.Error($"Failed to dispose:\n{err}");
		}
	}
}
