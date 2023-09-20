using System.Collections.Generic;

using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

using Pathfinder.Interop.Unmanaged;

namespace Pathfinder.Objects.Data;

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
		var ptr = this.Pointer.Data;
		var child = ptr->ChildObject;
		return child != null && child != ptr ? new WorldObject(child) : null;
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
		
		var firstSibling = child.NextSibling();
		var sibling = firstSibling;
		while (sibling != null && sibling.Pointer.Address != this.Pointer.Address && sibling.Pointer.Address != child.Pointer.Address) {
			yield return sibling;
			sibling = sibling.NextSibling();
			if (sibling?.Pointer.Address == firstSibling?.Pointer.Address)
				break;
		}
	}

	public IEnumerable<WorldObject> GetSiblings() {
		var sibling = NextSibling();
		
		while (sibling != null && sibling.Pointer.Address != this.Pointer.Address) {
			yield return sibling;
			sibling = sibling.NextSibling();
		}
	}
}
