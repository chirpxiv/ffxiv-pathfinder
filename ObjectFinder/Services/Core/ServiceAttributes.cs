using System;

namespace ObjectFinder.Services.Core;

[AttributeUsage(AttributeTargets.Class)]
public abstract class ServiceAttribute : Attribute { }

public class GlobalServiceAttribute : ServiceAttribute { }

public class ServiceEventAttribute : ServiceAttribute { }

public class ServiceStateAttribute : ServiceAttribute { }
