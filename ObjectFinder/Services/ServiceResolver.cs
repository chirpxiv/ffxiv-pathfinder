using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Dalamud.Logging;

using Microsoft.Extensions.DependencyInjection;

namespace ObjectFinder.Services;

public class ServiceResolver<A> where A : Attribute {
	// Constructor

	private readonly Type BaseType = typeof(A);
	
	private readonly IServiceCollection _services;

	public ServiceResolver(IServiceCollection _services) {
		this._services = _services;
	}
	
	// Resolver state

	private bool HasResolvedTypes;
	private readonly List<Type> Types = new();

	private void GetTypes() {
		if (this.HasResolvedTypes) return;

		try {
			var types = Assembly.GetExecutingAssembly()
				.GetTypes()
				.AsParallel()
				.Where(TypeHasBaseAttribute);
			this.Types.AddRange(types);
		} catch (Exception err) {
			PluginLog.Error($"Failed to resolve services for '{this.BaseType.Name}':\n{err}");
			throw;
		} finally {
			this.HasResolvedTypes = true;
		}
	}
	
	// Factory methods
	
	private List<Type> GetByAttribute<B>() where B : A {
		GetTypes();
		return this.Types.Where(TypeHasAttribute<B>).ToList();
	}

	public ServiceResolver<A> AddSingletons<B>() where B : A {
		GetByAttribute<B>().ForEach(type => this._services.AddSingleton(type));
		return this;
	}

	public ServiceResolver<A> AddScoped<B>() where B : A {
		GetByAttribute<B>().ForEach(type => this._services.AddScoped(type));
		return this;
	}
	
	// Service access

	public IEnumerable<ServiceDescriptor> GetServices<B>() where B : A => this._services
		.Where(desc => TypeHasAttribute<B>(desc.ServiceType));
	
	// Attribute helpers

	private bool TypeHasBaseAttribute(Type type) => type.CustomAttributes.Any(IsBaseAttribute);
	private bool IsBaseAttribute(CustomAttributeData data) => data.AttributeType.BaseType == this.BaseType;

	private static bool TypeHasAttribute<B>(Type type) => type.CustomAttributes.Any(IsAttribute<B>);
	private static bool IsAttribute<B>(CustomAttributeData data) => data.AttributeType == typeof(B);
}
