using System;
using Graphs.Abstract;
using UnityEngine;
using UnityEngine.Events;

namespace Graphs.Data
{
	[Serializable]
	public struct GraphSettings
	{
		public Color color;
		
		public UnityEvent<IGraph> valuesProvider;
		
		public string label;
		
		public float scaleMin;
		public float scaleMax;
		
		public bool autoScale;
	}
}