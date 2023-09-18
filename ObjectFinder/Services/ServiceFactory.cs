using System;

using Dalamud.Plugin;

using Microsoft.Extensions.DependencyInjection;

using ObjectFinder.Events;
using ObjectFinder.Events.Impl;
using ObjectFinder.Services.Attributes;

namespace ObjectFinder.Services; 

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

	private ServiceResolver<ServiceAttribute>? Resolver;
	
	private ServiceResolver<ServiceAttribute> GetResolver()
		=> this.Resolver ??= new ServiceResolver<ServiceAttribute>(this.Services);

	private ServiceResolver<ServiceAttribute> ConsumeResolver() {
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
			.AddScoped<ServiceStateAttribute>();
		return this;
	}

	public void Initialize(ServiceProvider provider) {
		var services = ConsumeResolver()
			.GetServices<GlobalServiceAttribute>();

		foreach (var service in services)
			provider.GetRequiredService(service.ServiceType);
	}

	public void Initialize<E>(ServiceProvider provider) where E : IEvent {
		Initialize(provider);
		provider.GetRequiredService<E>().Invoke();
	}
	
	// Disposal

	public void Dispose() => this.Resolver = null;
}
