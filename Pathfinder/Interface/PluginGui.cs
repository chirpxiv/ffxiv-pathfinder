using System;
using System.Linq;

using Dalamud.Interface;
using Dalamud.Interface.Windowing;

using Microsoft.Extensions.DependencyInjection;

using Pathfinder.Events;
using Pathfinder.Interface.Windows;
using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Interface; 

[GlobalService]
public class PluginGui : IDisposable {
	private readonly IUiBuilder _ui;

	private readonly IServiceScope _scope;
	private readonly WindowSystem _windows = new("Pathfinder");
		
	public PluginGui(IServiceProvider services, IUiBuilder ui, InitEvent init) {
		this._ui = ui;
		
		this._scope = services.CreateScope();
		init.Subscribe(this.OnInit);
	}
	
	// Initialization
	
	private Action Draw => this._windows.Draw;

	private void OnInit() {
		this.AddWindow<MainWindow>();
		this.AddWindow<OverlayWindow>();
		this._ui.Draw += this.Draw;
		this._ui.OpenMainUi += this.OpenMainUi;
		this._ui.OpenConfigUi += this.OpenConfigUi;
		this._ui.DisableGposeUiHide = true;
		this._ui.DisableCutsceneUiHide = true;
	}
	
	// Window management

	private T AddWindow<T>() where T : Window {
		var window = this._scope.ServiceProvider.GetRequiredService<T>();
		this._windows.AddWindow(window);
		return window;
	}

	public T GetWindow<T>() where T : Window {
		if (this._windows.Windows.FirstOrDefault(w => w is T) is T window)
			return window;
		return this.AddWindow<T>();
	}
	
	// Events

	private void OpenMainUi() => this.GetWindow<MainWindow>().Toggle();

	private void OpenConfigUi() => this.GetWindow<ConfigWindow>().Toggle();
	
	// Disposal

	public void Dispose() {
		this._ui.Draw -= this.Draw;
		this._ui.OpenMainUi -= this.OpenMainUi;
		this._ui.OpenConfigUi -= this.OpenConfigUi;
		this._scope.Dispose();
	}
}
