using UnityEngine;

namespace Graphs.Abstract
{
	public interface IGraphGroup
	{
		string title { get; }

		void AddGraph(IGraph graph);
		void RemoveGraph(IGraph graph);

		void SetSyncMode(bool syncEnabled);
		
		void ClearMarkers();
		IGraphGroup AddMarkerAt(float value, Color markerColor);
		
		void SwitchGroupPausedState();
		void SetTitle(string title);
	}
}