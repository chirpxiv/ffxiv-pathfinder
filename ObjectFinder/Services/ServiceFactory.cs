using System;

using Dalamud.Plugin;

using Microsoft.Extensions.DependencyInjection;

namespace ObjectFinder.Services; 

public class ServiceFactory {
	// Services state
	
	private readonly ServiceCollection Services = new();

	// Provider instantiation
	
	private readonly static ServiceProviderOptions ProviderOptions = new() {
		ValidateScopes = true,
        ValidateOnBuild = true
	};

	public ServiceProvider CreateProvider()
		=> this.Services.BuildServiceProvider(ProviderOptions);
	
	// Factory methods

	public ServiceFactory AddDalamud(DalamudPluginInterface api) {
		var container = new DalamudServices(api);
		container.AddServices(this.Services);
		return this;
	}

	public ServiceFactory AddResolveType<A>() where A : Attribute {
		var resolver = new ServiceResolver<A>();
		resolver.GetTypes().ForEach(type => this.Services.AddSingleton(type));
		return this;
	}
}
