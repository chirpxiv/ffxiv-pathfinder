using Pathfinder.Objects.Data;

namespace Pathfinder.Config.Data; 

public class ObjectFilters {
	public ObjectFilterFlags Flags;

	public (bool Enabled, float Value) MinRadius = (true, 1.0f);
	public (bool Enabled, float Value) MaxRadius = (true, 5.0f);
}
