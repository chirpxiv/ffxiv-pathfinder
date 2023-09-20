using System;

using Dalamud.Logging;
using Dalamud.Plugin;

using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Config; 

[GlobalService]
public class ConfigService : IDisposable {
	private readonly DalamudPluginInterface _api;

	private ConfigFile _config = null!;
	
	public ConfigService(DalamudPluginInterface _api) {
		this._api = _api;
	}

	public ConfigFile Load() {
		var cfgBase = this._api.GetPluginConfig();
		return this._config = cfgBase?.Version switch {
			ConfigFile.CurrentVersion => (ConfigFile)cfgBase,
			// TODO: Upgrade
			_ or null => new ConfigFile()
		};
	}

	public ConfigFile Get() {
		if (this._config == null || this.IsDisposed)
			throw new Exception("Attempted to retrieve config, but it was invalid.");
		return this._config;
	}

	public void Save() {
		try {
			var cfg = this.Get();
			this._api.SavePluginConfig(cfg);
		} catch (Exception err) {
			PluginLog.Error($"Failed to save configuration:\n{err}");
		}
	}
	
	// IDisposable

	private bool IsDisposed;

	public void Dispose() {
		if (this.IsDisposed) return;
		Save();
		this.IsDisposed = true;
	}
}
