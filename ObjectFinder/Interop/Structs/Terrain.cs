using System.Runtime.InteropServices;

using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;

namespace ObjectFinder.Interop.Structs; 

[StructLayout(LayoutKind.Explicit, Size = 0x1F0)]
public struct Terrain {
	[FieldOffset(0x90)] public unsafe ResourceHandle* ResourceHandle;
}
