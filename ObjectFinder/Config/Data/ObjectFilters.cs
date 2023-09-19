using ObjectFinder.Objects.Data;

namespace ObjectFinder.Config.Data; 

public class ObjectFilters {
	public ObjectFilterFlags Flags;

	public (bool Enabled, float Value) MinRadius = (true, 1.0f);
	public (bool Enabled, float Value) MaxRadius = (true, 5.0f);
}
