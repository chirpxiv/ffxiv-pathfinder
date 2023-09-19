using System;
using System.Runtime.CompilerServices;

namespace Pathfinder.Interop.Unmanaged; 

public class Pointer<T> where T : unmanaged {
	public nint Address { get; set; } = nint.Zero;
	
	public unsafe T* Data {
		get {
			if (this.IsNull)
				throw new Exception("Attempted to access null pointer.");
			return (T*)this.Address;
		}
		set => this.Address = (nint)value;
	}
	
	public Pointer(nint address) => this.Address = address;
	public unsafe Pointer(T* data) => this.Data = data;
	
	public bool IsNull => this.Address == nint.Zero;
	
	public Pointer<U> Cast<U>() where U : unmanaged => new(this.Address);
}
