using Dalamud.IoC;
using Dalamud.Game;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Pathfinder.Services; 

internal sealed class DalamudServices {
	private readonly DalamudPluginInterface _api;
	[PluginService] private ICommandManager _cmd { get; set; } = null!;
	[PluginService] private Framework _framework { get; set; } = null!;
	[PluginService] private ChatGui _chat { get; set; } = null!;
	[PluginService] private IGameGui _gui { get; set; } = null!;
	[PluginService] private IClientState _state { get; set; } = null!;

	internal DalamudServices(DalamudPluginInterface api) {
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
