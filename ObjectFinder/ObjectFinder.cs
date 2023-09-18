using System;

using Dalamud.Logging;
using Dalamud.Plugin;

using Microsoft.Extensions.DependencyInjection;

using ObjectFinder.Events;
using ObjectFinder.Services.Core;

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
