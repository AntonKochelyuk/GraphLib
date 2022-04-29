using Graphs.Abstract;
using UnityEngine;

namespace Graphs.Implementations
{
	public class GraphTest : MonoBehaviour
	{
		private GraphGroup m_graphGroup;

		private IGraph m_graph1;
		private IGraph m_graph2;
		private IGraph m_graph3;
		private IGraph m_graph4;
		
		private void Start()
		{
			Application.targetFrameRate = 60;
			
			var graphManager = GraphsManager.instance;
			m_graphGroup = graphManager.defaultGroup;
			
			m_graph1 = LineGraphBuilder.Create()
										 .WithValuesGetter(GetTime)
										 .WithColor(Color.red)
										 .WithCustomScale(0, 1)
										 .WithLabel("Time")
										 .Build();
			
			m_graph2 = LineGraphBuilder.Create()
										 .WithValuesGetter(GetSin)
										 .WithColor(Color.blue)
										 .WithCustomScale(-1f, 1f)
										 .WithLabel("Sin")
										 .Build();
			
			m_graph3 = LineGraphBuilder.Create()
										 .WithValuesGetter(GetCos)
										 .WithColor(Color.green)
										 .WithCustomScale(-1f, 1f)
										 .WithLabel("Cos")
										 .Build();
			
			m_graph4 = LineGraphBuilder.Create()
										 .WithValuesGetter(FPS)
										 .WithColor(Color.yellow)
										 .WithCustomScale(10, 65)
										 .WithLabel("FPS")
										 .Build();
			
			m_graphGroup.AddGraph(m_graph1);
			m_graphGroup.AddGraph(m_graph2);
			m_graphGroup.AddGraph(m_graph3);
			m_graphGroup.AddGraph(m_graph4);
		}

		public void SetSinGraph(IGraph graph)
		{
			graph.PushValue(GetSin());
		}

		public void SetCosGraph(IGraph graph)
		{
			graph.PushValue(GetCos());
		}

		private void OnDestroy()
		{
			m_graphGroup.AddGraph(m_graph1);
			m_graphGroup.AddGraph(m_graph2);
			m_graphGroup.AddGraph(m_graph3);
			m_graphGroup.AddGraph(m_graph4);
		}

		private float GetTime()
		{
			return Time.time % 4f;
		}

		private float GetSin()
		{
			return Mathf.Sin(Time.time * 4);
		}
		
		private float GetCos()
		{
			return Mathf.Cos(Time.time);
		}

		private float FPS()
		{
			return 1 / Time.deltaTime;
		}
	}
}