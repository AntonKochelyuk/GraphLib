using System;
using Graphs.Abstract;
using UnityEngine;
using UnityEngine.Events;

namespace Graphs.Data
{
	[Serializable]
	public struct GraphData
	{
		public Color color;
		
		public UnityEvent<IGraph> valuesSetter;
		
		public string label;
		
		public float scaleMin;
		public float scaleMax;
		
		public bool autoScale;
	}
}