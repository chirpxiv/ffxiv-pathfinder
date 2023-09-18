﻿using System;
using System.Linq;

using Dalamud.Interface;
using Dalamud.Interface.Windowing;

using Microsoft.Extensions.DependencyInjection;

using ObjectFinder.Events;
using ObjectFinder.Services.Attributes;

namespace ObjectFinder.Interface; 

[GlobalService]
public class PluginGui : IDisposable {
	private readonly UiBuilder _ui;

	private readonly IServiceScope _scope;
	private readonly WindowSystem _windows = new("Object Finder");
		
	public PluginGui(IServiceProvider _services, UiBuilder _ui, InitEvent _init) {
		this._ui = _ui;
		
		this._scope = _services.CreateScope();
		_init.Subscribe(OnInit);
	}
	
	// Initialization
	
	private Action Draw => this._windows.Draw;

	private void OnInit() {
		this._ui.Draw += this.Draw;
	}
	
	// Window management

	public T GetWindow<T>() where T : Window {
		if (this._windows.Windows.FirstOrDefault(w => w is T) is T window)
			return window;
		window = this._scope.ServiceProvider.GetRequiredService<T>();
		this._windows.AddWindow(window);
		return window;
	}
	
	// Disposal

	public void Dispose() {
		this._ui.Draw -= this.Draw;
		this._scope.Dispose();
	}
}