using System;

namespace ObjectFinder.Services.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public abstract class ServiceAttribute : Attribute { }

public class GlobalServiceAttribute : ServiceAttribute { }

public class ServiceEventAttribute : ServiceAttribute { }

public class LocalServiceAttribute : ServiceAttribute { }
