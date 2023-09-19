using System;

using Pathfinder.Events.Impl;
using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Events;

[ServiceEvent]
public class InitEvent : EventBase<Action> { }
