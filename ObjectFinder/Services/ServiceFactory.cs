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
		new DalamudServices(api).AddServices(this.Services);
		return this;
	}
}
