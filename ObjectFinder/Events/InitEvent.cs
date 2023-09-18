using System;

using ObjectFinder.Events.Impl;
using ObjectFinder.Services.Attributes;

namespace ObjectFinder.Events;

[ServiceEvent]
public class InitEvent : EventBase<Action> { }
