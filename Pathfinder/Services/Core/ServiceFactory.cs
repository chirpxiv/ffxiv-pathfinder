﻿using System;

using Dalamud.Plugin;

using Microsoft.Extensions.DependencyInjection;

using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Services.Core; 

public class ServiceFactory : IDisposable {
	// Services state
	
	private readonly ServiceCollection Services = new();

	// Provider instantiation
	
	private readonly static ServiceProviderOptions ProviderOptions = new() {
		ValidateScopes = true,
		ValidateOnBuild = true
	};

	public ServiceProvider CreateProvider()
		=> this.Services.BuildServiceProvider(ProviderOptions);
	
	// Resolver

	private ServiceResolver? Resolver;
	
	private ServiceResolver GetResolver()
		=> this.Resolver ??= new ServiceResolver(this.Services);

	private ServiceResolver ConsumeResolver() {
		var inst = GetResolver();
		if (inst == null) throw new Exception("Invalid resolver state.");
		this.Resolver = null;
		return inst;
	}
	
	// Factory methods

	public ServiceFactory AddDalamud(DalamudPluginInterface api) {
		var container = new DalamudServices(api);
		container.AddServices(this.Services);
		return this;
	}

	public ServiceFactory ResolveAll() {
		this.GetResolver()
			.AddSingletons<GlobalServiceAttribute>()
			.AddSingletons<ServiceEventAttribute>()
			.AddLocal<LocalServiceAttribute>();
		return this;
	}

	public void Initialize(ServiceProvider provider) {
		var services = ConsumeResolver()
			.GetServices<GlobalServiceAttribute>();

		foreach (var service in services)
			provider.GetRequiredService(service.ServiceType);
	}
	
	// Disposal

	public void Dispose() => this.Resolver = null;
}
