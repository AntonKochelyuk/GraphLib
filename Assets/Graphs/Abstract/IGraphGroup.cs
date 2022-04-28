using System;
using UnityEngine;

namespace Graphs.Abstract
{
	public interface IGraphGroup
	{
		string title { get; }

		IDisposable AddGraph(IGraph graph);

		void SetSyncMode(bool syncEnabled);
		
		void ClearMarkers();
		IGraphGroup AddMarkerAt(float value, Color markerColor);
		
		void SwitchGroupPausedState();
		void SetTitle(string title);
	}
}