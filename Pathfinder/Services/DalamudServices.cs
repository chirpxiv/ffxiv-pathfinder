﻿using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Pathfinder.Services; 

internal sealed class DalamudServices {
	private readonly IDalamudPluginInterface _api;
	[PluginService] private ICommandManager _cmd { get; set; } = null!;
	[PluginService] private IFramework _framework { get; set; } = null!;
	[PluginService] private IChatGui _chat { get; set; } = null!;
	[PluginService] private IGameGui _gui { get; set; } = null!;
	[PluginService] private IClientState _state { get; set; } = null!;

	internal DalamudServices(IDalamudPluginInterface api) {
		this._api = api;
		api.Inject(this);
	}

	internal void AddServices(ServiceCollection services) => services
		.AddSingleton(this._api)
		.AddSingleton(this._api.UiBuilder)
		.AddSingleton(this._cmd)
		.AddSingleton(this._framework)
		.AddSingleton(this._chat)
		.AddSingleton(this._gui)
		.AddSingleton(this._state);
}
