using System;
using Graphs.Abstract;
using Graphs.Data;
using UnityEngine;
using UnityEngine.Events;

namespace Graphs.Implementations
{
	public class GraphCreator : MonoBehaviour
	{
		[SerializeField]
		private GraphGroup m_group;

		[SerializeField]
		private GraphData m_graphData;
		
		private float m_value;
		private IGraph m_graph;

		private IDisposable m_graphSubscription;
		
		private void Start()
		{
			m_graph = LineGraphBuilder.FromGraphData(m_graphData)
									  .Build();

			m_graphSubscription = m_group.AddGraph(m_graph);
		}

		private void OnDestroy()
		{
			m_graphSubscription?.Dispose();
		}
		
		private void Update()
		{
			m_graphData.valuesSetter.Invoke(m_graph);
		}
	}
}