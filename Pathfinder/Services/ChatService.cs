using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling;

using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Services; 

[GlobalService]
public class ChatService {
	private readonly IChatGui _chat;
	
	public ChatService(IChatGui _chat) {
		this._chat = _chat;
	}

	public void PrintMessage(string text) {
		var msgString = new SeStringBuilder()
			.AddUiForeground("[Pathfinder] ", 1)
			.AddText(text)
			.Build();
		
		this._chat.Print(msgString);
	}
}
