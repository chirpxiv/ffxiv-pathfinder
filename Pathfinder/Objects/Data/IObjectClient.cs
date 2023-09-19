using System;
using System.Collections.Generic;

namespace Pathfinder.Objects.Data;

public interface IObjectClient : IDisposable {
	public IEnumerable<ObjectInfo> GetObjects();
}
