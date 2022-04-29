using Graphs.Abstract;
using Graphs.Data;
using UnityEngine;

namespace Graphs.Implementations
{
	public class GraphCreator : MonoBehaviour
	{
		[SerializeField]
		private GraphGroup m_group;

		[SerializeField]
		private GraphSettings m_graphData;
		
		private float m_value;
		private IGraph m_graph;
		
		private void Start()
		{
			m_graph = LineGraphBuilder.FromGraphData(m_graphData)
									  .Build();

			m_group.AddGraph(m_graph);
		}

		private void OnDestroy()
		{
			m_group.RemoveGraph(m_graph);
		}
		
		private void Update()
		{
			m_graphData.valuesProvider.Invoke(m_graph);
		}
	}
}