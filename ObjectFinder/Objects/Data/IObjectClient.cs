using System;
using System.Collections.Generic;

namespace ObjectFinder.Objects.Data;

public interface IObjectClient : IDisposable {
	public IEnumerable<ObjectInfo> GetObjects();
}
