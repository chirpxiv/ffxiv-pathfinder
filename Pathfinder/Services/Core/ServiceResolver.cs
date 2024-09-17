using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Dalamud.Plugin.Services;

using Microsoft.Extensions.DependencyInjection;

using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Services.Core;

public class ServiceResolver {
	// Constructor

	private readonly Type BaseType = typeof(ServiceAttribute);

	private readonly IPluginLog _log;
	private readonly IServiceCollection _services;

	public ServiceResolver(
		IPluginLog log,
		IServiceCollection services
	) {
		this._log = log;
		this._services = services;
	}
	
	// Resolver state

	private bool HasResolvedTypes;
	private readonly List<Type> Types = new();

	private void GetTypes() {
		if (this.HasResolvedTypes) return;

		try {
			var types = Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(TypeHasBaseAttribute);
			this.Types.AddRange(types);
		} catch (Exception err) {
			this._log.Error($"Failed to resolve services for '{this.BaseType.Name}':\n{err}");
			throw;
		} finally {
			this.HasResolvedTypes = true;
		}
	}
	
	// Factory methods
	
	private IEnumerable<Type> GetByAttribute<A>() where A : ServiceAttribute {
		this.GetTypes();
		return this.Types.Where(TypeHasAttribute<A>);
	}

	public ServiceResolver AddSingletons<A>() where A: ServiceAttribute {
		foreach (var type in this.GetByAttribute<A>())
			this._services.AddSingleton(type);
		return this;
	}

	public ServiceResolver AddLocal<A>() where A : LocalServiceAttribute {
		foreach (var type in this.GetByAttribute<A>()) {
			var attr = type.GetCustomAttribute<A>()!;
			if (attr.HasFlag(ServiceFlags.Transient))
				this._services.AddTransient(type);
			else
				this._services.AddScoped(type);
		}
		return this;
	}

	public ServiceResolver AddLocal() => this.AddLocal<LocalServiceAttribute>();
	
	// Service access

	public IEnumerable<ServiceDescriptor> GetServices<A>() where A: ServiceAttribute => this._services
		.Where(desc => TypeHasAttribute<A>(desc.ServiceType));
	
	// Attribute helpers

	private bool TypeHasBaseAttribute(Type type) => type.CustomAttributes.Any(this.IsBaseAttribute);
	private bool IsBaseAttribute(CustomAttributeData data) => data.AttributeType.BaseType == this.BaseType;

	private static bool TypeHasAttribute<B>(Type type) => type.CustomAttributes.Any(IsAttribute<B>);
	private static bool IsAttribute<B>(CustomAttributeData data) => data.AttributeType == typeof(B);
}
