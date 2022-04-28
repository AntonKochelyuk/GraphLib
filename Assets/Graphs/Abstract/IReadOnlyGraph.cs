using UnityEngine;

namespace Graphs.Abstract
{
	public interface IReadOnlyGraph
	{
		event System.Action<float> newValueReceivedEvent;

		string label { get; }
		
		float minScale { get; }
		float maxScale { get; }

		Color graphColor { get; }
		
		float GetValueAt(int index);
	}
}