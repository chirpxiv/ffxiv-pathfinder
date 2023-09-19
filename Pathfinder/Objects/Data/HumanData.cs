namespace Pathfinder.Objects.Data;

public enum Clan : byte {
	Unknown,
	Midlander,
	Highlander,
	Wildwood,
	Duskwight,
	Plainsfolk,
	Dunesfolk,
	SunSeeker,
	MoonKeeper,
	Seawolf,
	Hellsguard,
	Raen,
	Xaela,
	Helion,
	Lost,
	Rava,
	Veena
}

public class HumanData {
	public Clan Clan;
	public byte Gender;
}
