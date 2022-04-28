using System;
using Graphs.Implementations;
using UnityEngine;

namespace Graphs.Abstract
{
	public interface IGraphsManager
	{
		GraphGroup defaultGroup { get; }

		GraphGroup GetOrCreateGroup(string name, Transform parent = null);
		GraphGroup GetGroup(string name);

		IDisposable RegisterGroup(GraphGroup group);
	}
}