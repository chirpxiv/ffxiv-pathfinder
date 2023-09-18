using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ObjectFinder.Services;

public class ServiceResolver<A> where A : Attribute {
	private readonly Type Type = typeof(A);

	public List<Type> GetTypes()
		=> Assembly.GetExecutingAssembly()
		.GetTypes()
		.Where(type => type.CustomAttributes.Any(Match))
		.ToList();

	private bool Match(CustomAttributeData data)
		=> data.AttributeType == this.Type;
}
