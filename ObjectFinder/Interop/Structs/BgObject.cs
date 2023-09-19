using System.Runtime.InteropServices;

using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;

namespace ObjectFinder.Interop.Structs; 

[StructLayout(LayoutKind.Explicit, Size = 0xD0)]
public struct BgObject {
	[FieldOffset(0x90)] public unsafe ResourceHandle* ResourceHandle;
}
