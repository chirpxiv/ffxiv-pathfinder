using System;
using System.Numerics;
using System.Collections.Generic;

using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Object = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object;
using ModelType = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CharacterBase.ModelType;

using Pathfinder.Config.Data;
using Pathfinder.Interop.Structs;
using Pathfinder.Interop.Unmanaged;

namespace Pathfinder.Objects.Data;

public class ObjectInfo {
	public nint Address;
	
	public ObjectType Type;
	public ModelType ModelType;
	
	public WorldObjectType FilterFlags;
	public WorldObjectType FilterType;
	
	public Vector3 Position;
	public float Distance;

	public HumanData? HumanData;
	
	public readonly List<ModelData> Models = new();

	public string GetItemTypeString() {
		var type = this.Type;

		if (type == ObjectType.CharacterBase) {
			var mdlType = this.ModelType;
			var result = mdlType.ToString();
			if (mdlType == ModelType.Human && this.HumanData is { } data)
				result += $" ({data.Clan} {(data.Gender == 0 ? '♂' : '♀')})";
			return result;
		}

		return type.ToString();
	}
	
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
				this.FilterType = WorldObjectType.BgObject;
				break;
			case ObjectType.Terrain:
				ReadTerrain(ptr.Cast<Terrain>());
				this.FilterType = WorldObjectType.Terrain;
				break;
			case ObjectType.CharacterBase:
				ReadCharaBase(ptr.Cast<CharacterBase>());
				this.FilterFlags = WorldObjectType.Chara;
				this.FilterType = this.ModelType switch {
					ModelType.Human => WorldObjectType.Human,
					ModelType.DemiHuman => WorldObjectType.DemiHuman,
					ModelType.Monster => WorldObjectType.Monster,
					ModelType.Weapon => WorldObjectType.Weapon,
					_ => WorldObjectType.None
				};
				break;
			default:
				break;
		}

		this.FilterFlags |= this.FilterType;
	}
	
	// Models

	private void AddModel(string path, int slot = 0, bool human = false, nint address = 0)
		=> this.Models.Add(new ModelData { Path = path, Slot = slot, IsHuman = human, Address = address});
	
	// BgObject handler

	private unsafe void ReadBgObject(Pointer<BgObject> ptr) {
		var resource = ptr.Data->ResourceHandle;
		if (resource != null)
			AddModel(resource->FileName.ToString());
	}
	
	// Terrain handler

	private unsafe void ReadTerrain(Pointer<Terrain> ptr) {
		var resource = ptr.Data->ResourceHandle;
		if (resource != null)
			AddModel(resource->FileName.ToString());
	}
	
	// CharacterBase handler

	private unsafe void ReadCharaBase(Pointer<CharacterBase> ptr) {
		this.ModelType = ptr.Data->GetModelType();

		var isHuman = this.ModelType == ModelType.Human;
		if (isHuman) ReadHuman(ptr.Cast<Human>());
		
		if (ptr.Data->Models == null) return;
		
		var modelCt = ptr.Data->SlotCount;
		for (var i = 0; i < modelCt; i++) {
			var model = ptr.Data->Models[i];
			if (model == null || model->ModelResourceHandle == null) continue;
			
			var path =  model->ModelResourceHandle->ResourceHandle.FileName.ToString();
			AddModel(path, i, isHuman, (nint)model);
		}
	}

	private unsafe void ReadHuman(Pointer<Human> ptr) {
		var custom = ptr.Data->Customize;
		this.HumanData = new HumanData {
			Clan = (Clan)custom.Clan,
			Gender = custom.Sex
		};
	}
}
