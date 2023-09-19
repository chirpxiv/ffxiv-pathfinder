using System.Collections.Generic;

using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

using ObjectFinder.Interop.Unmanaged;
using ObjectFinder.Objects.Data;

namespace ObjectFinder.Interop.Structs;

// Wrapper around Object* with null checks enforced by the Pointer<T> class.

public class WorldObject {
	private readonly Pointer<Object> Pointer;

	public WorldObject(Pointer<Object> ptr)
		=> this.Pointer = ptr;

	public unsafe WorldObject(Object* ptr)
		=> this.Pointer = new Pointer<Object>(ptr);
	
	// Data wrappers

	public unsafe ObjectType ObjectType => this.Pointer.Data->GetObjectType();
	
	// Factory methods

	public ObjectInfo GetObjectInfo() => new(this.Pointer);
	
	// Enumerate children & siblings
	
	private unsafe WorldObject? GetFirstChild() {
		if (this.Pointer.IsNull) return null;
		var child = this.Pointer.Data->ChildObject;
		return child != null ? new WorldObject(child) : null;
	}

	private unsafe WorldObject? NextSibling() {
		if (this.Pointer.IsNull) return null;
		var ptr = this.Pointer.Data;
		var sibling = ptr->NextSiblingObject;
		if (sibling == null || sibling == ptr)
			return null;
		return new WorldObject(sibling);
	}

	public IEnumerable<WorldObject> GetChildren() {
		var child = GetFirstChild();
		if (child == null) yield break;
		yield return child;

		var i = 0;
		var sibling = child.NextSibling();
		while (sibling != null && sibling.Pointer.Address != this.Pointer.Address && i++ < 100) {
			yield return sibling;
			sibling = sibling.NextSibling();
		}
	}
}
