namespace Pathfinder.Objects.Data;

public enum ModelSlot {
	Hat,
	Top,
	Gloves,
	Legs,
	Shoes,
	Ears,
	Neck,
	Wrists,
	RingRight,
	RingLeft,
	Hair,
	Face,
	TailEars,
	
	Max = TailEars,
	Unknown = -1
}

public class ModelData {
	public bool IsHuman;

	public nint Address;
	public required string Path;
	public int Slot;

	public string GetSlotString() => this.IsHuman switch {
		true when this.Slot <= (int)ModelSlot.Max => (ModelSlot)this.Slot switch {
			ModelSlot.Unknown => string.Empty,
			ModelSlot.RingRight => "Ring (Right)",
			ModelSlot.RingLeft => "Ring (Left)",
			ModelSlot.TailEars => "Tail/Ears",
			var slot => slot.ToString()
		},
		_ => $"Slot {this.Slot + 1}"
	};
}
