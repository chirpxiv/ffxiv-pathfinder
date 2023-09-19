using System;

namespace ObjectFinder.Services.Core.Attributes;

// Flags

[Flags]
public enum ServiceFlags {
	None = 0,
	Transient = 1
}

// Attributes

[AttributeUsage(AttributeTargets.Class)]
public abstract class ServiceAttribute : Attribute { }

public class GlobalServiceAttribute : ServiceAttribute { }

public class ServiceEventAttribute : ServiceAttribute { }

public class LocalServiceAttribute : ServiceAttribute {
	private readonly ServiceFlags Flags;

	public bool HasFlag(ServiceFlags flag) => this.Flags.HasFlag(flag);

	public LocalServiceAttribute(ServiceFlags flags = ServiceFlags.None)
		=> this.Flags = flags;
}
