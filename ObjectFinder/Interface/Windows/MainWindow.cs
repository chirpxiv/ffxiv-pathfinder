using Dalamud.Interface.Windowing;

using ImGuiNET;

using ObjectFinder.Services.Attributes;

namespace ObjectFinder.Interface.Windows; 

[LocalService]
public class MainWindow : Window {
	public MainWindow() : base("Object Finder") {}

	public override void Draw() {
		ImGui.Text("haiii :3");
	}
}
