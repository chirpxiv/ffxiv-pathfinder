using System;

using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Config; 

[GlobalService]
public class ConfigService : IDisposable {
	private readonly IDalamudPluginInterface _api;
	private readonly IPluginLog _log;

	private ConfigFile _config = null!;
	
	public ConfigService(
		IDalamudPluginInterface api,
		IPluginLog log
	) {
		this._api = api;
		this._log = log;
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
			this._log.Error($"Failed to save configuration:\n{err}");
		}
	}
	
	// IDisposable

	private bool IsDisposed;

	public void Dispose() {
		if (this.IsDisposed) return;
		this.Save();
		this.IsDisposed = true;
	}
}
