using System;
using System.Collections.Generic;

using Dalamud.Plugin.Services;
using Dalamud.Game.Command;
using HandlerDelegate = Dalamud.Game.Command.CommandInfo.HandlerDelegate;

using Pathfinder.Events;
using Pathfinder.Interface;
using Pathfinder.Interface.Windows;
using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Services; 

[GlobalService]
public class CommandService : IDisposable {
	private readonly ICommandManager _cmd;
	private readonly PluginGui _gui;
	
	public CommandService(ICommandManager _cmd, PluginGui _gui, InitEvent _init) {
		this._cmd = _cmd;
		this._gui = _gui;
		_init.Subscribe(OnInit);
	}
	
	// Initialization
    
	private readonly HashSet<string> Commands = new();

	private void OnInit() {
		BuildCommand("/pathfinder", OnCommand)
			.SetMessage("Toggles the main Pathfinder window.")
			.AddAliases("/pathfind", "/findpath", "/findpaths")
			.Create();
	}
	
	private void Add(string name, CommandInfo info) {
		if (this.Commands.Add(name))
			this._cmd.AddHandler(name, info);
	}

	private CommandFactory BuildCommand(string name, HandlerDelegate handler)
		=> new(this, name, handler);

	// Command handlers

	private void OnCommand(string _command, string arguments)
		=> this._gui.GetWindow<MainWindow>().Toggle();
	
	// Disposal

	public void Dispose() {
		foreach (var cmdName in this.Commands)
			this._cmd.RemoveHandler(cmdName);
	}
	
	// Factory

	private class CommandFactory {
		private readonly CommandService _cmd;

		private readonly string Name;
		private readonly List<string> Alias = new();

		private readonly HandlerDelegate Handler;

		private bool ShowInHelp;
		private string HelpMessage = string.Empty;
		
		public CommandFactory(CommandService _cmd, string name, HandlerDelegate handler) {
			this._cmd = _cmd;
			
			this.Name = name;
			this.Handler = handler;
		}
		
		// Factory methods
        
		public CommandFactory SetMessage(string message) {
			this.ShowInHelp = true;
			this.HelpMessage = message;
			return this;
		}

		public CommandFactory AddAlias(string alias) {
			this.Alias.Add(alias);
			return this;
		}

		public CommandFactory AddAliases(params string[] aliases) {
			this.Alias.AddRange(aliases);
			return this;
		}

		public void Create() {
			this._cmd.Add(this.Name, BuildCommandInfo());
			this.Alias.ForEach(CreateAlias);
		}

		private void CreateAlias(string alias) {
			this._cmd.Add(alias, new CommandInfo(this.Handler) {
				ShowInHelp = false
			});
		}

		// CommandInfo

		private CommandInfo BuildCommandInfo() {
			var message = this.HelpMessage;
			if (this.HelpMessage != string.Empty && this.Alias.Count > 0) {
				var padding = new string(' ', this.Name.Length * 2);
				message += $"\n{padding} (Aliases: {string.Join(", ", this.Alias)})";
			}

			return new CommandInfo(this.Handler) {
				ShowInHelp = this.ShowInHelp,
				HelpMessage = message
			};
		}
	}
}
