using System;
using System.Numerics;
using System.Collections.Generic;

using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Object = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object;
using ModelType = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CharacterBase.ModelType;

using ObjectFinder.Interop.Structs;
using ObjectFinder.Interop.Unmanaged;

namespace ObjectFinder.Objects.Data;

[Flags]
public enum ObjectFilterFlags {
	None = 0,
	
	BgObject = 1,
	Terrain = 2,
	Chara = 4,
	__Vfx = 8, // Reserved
	
	Human = Chara | 16,
	DemiHuman = Chara | 32,
	Monster = Chara | 64,
	Weapon = Chara | 128
}

public class ObjectInfo {
    public nint Address;
	
	public ObjectType Type;
	public ModelType ModelType;
	public ObjectFilterFlags FilterType;
	
	public Vector3 Position;
	public float Distance; // Stub
	
	public readonly List<string> ResourcePaths = new();
	
	// Factory
	
	public unsafe ObjectInfo(Pointer<Object> ptr) {
		if (ptr == null)
			throw new Exception("Null pointer passed into ObjectInfo. This should never happen!");
        
		this.Address = ptr.Address;
		
		this.Type = ptr.Data->GetObjectType();
		this.Position = ptr.Data->Position;

		switch (this.Type) {
			case ObjectType.BgObject:
                ReadBgObject(ptr.Cast<BgObject>());
				this.FilterType = ObjectFilterFlags.BgObject;
				break;
			case ObjectType.Terrain:
				ReadTerrain(ptr.Cast<Terrain>());
				this.FilterType = ObjectFilterFlags.Terrain;
				break;
			case ObjectType.CharacterBase:
				ReadCharaBase(ptr.Cast<CharacterBase>());
				this.FilterType = this.ModelType switch {
					ModelType.Human => ObjectFilterFlags.Human,
					ModelType.DemiHuman => ObjectFilterFlags.DemiHuman,
					ModelType.Monster => ObjectFilterFlags.Monster,
					ModelType.Weapon => ObjectFilterFlags.Weapon,
					_ => ObjectFilterFlags.Chara
				};
				break;
			default:
				break;
		}
	}
	
	// BgObject handler

	private unsafe void ReadBgObject(Pointer<BgObject> ptr) {
		var resource = ptr.Data->ResourceHandle;
		if (resource != null)
			this.ResourcePaths.Add(resource->FileName.ToString());
	}
	
	// Terrain handler

	private unsafe void ReadTerrain(Pointer<Terrain> ptr) {
		var resource = ptr.Data->ResourceHandle;
		if (resource != null)
			this.ResourcePaths.Add(resource->FileName.ToString());
	}
	
	// CharacterBase handler

	private unsafe void ReadCharaBase(Pointer<CharacterBase> ptr) {
		this.ModelType = ptr.Data->GetModelType();
	}
}
