using System;
using Graphs.Abstract;
using UnityEngine;

namespace Graphs.Implementations
{
	public class GraphTest : MonoBehaviour
	{
		private IDisposable m_graphSubscription1;
		private IDisposable m_graphSubscription2;
		private IDisposable m_graphSubscription3;
		private IDisposable m_graphSubscription4;
		
		private void Start()
		{
			Application.targetFrameRate = 60;
			
			var graphManager = GraphsManager.instance;
			var group = graphManager.defaultGroup;
			
			var graph1 = LineGraphBuilder.Create()
										 .WithValuesGetter(GetRandom)
										 .WithColor(Color.red)
										 .WithCustomScale(0, 100f)
										 .WithLabel("Time")
										 .Build();
			
			var graph2 = LineGraphBuilder.Create()
										 .WithValuesGetter(GetSin)
										 .WithColor(Color.blue)
										 .WithCustomScale(-1f, 1f)
										 .WithLabel("Sin")
										 .Build();
			
			var graph3 = LineGraphBuilder.Create()
										 .WithValuesGetter(GetCos)
										 .WithColor(Color.green)
										 .WithCustomScale(-1f, 1f)
										 .WithLabel("Cos")
										 .Build();
			
			var graph4 = LineGraphBuilder.Create()
										 .WithValuesGetter(FPS)
										 .WithColor(Color.yellow)
										 .WithCustomScale(10, 65)
										 .WithLabel("FPS")
										 .Build();
			
			m_graphSubscription1 = group.AddGraph(graph1);
			m_graphSubscription2 = group.AddGraph(graph2);
			m_graphSubscription3 = group.AddGraph(graph3);
			m_graphSubscription4 = group.AddGraph(graph4);
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
			m_graphSubscription1?.Dispose();
			m_graphSubscription2?.Dispose();
			m_graphSubscription3?.Dispose();
			m_graphSubscription4?.Dispose();
		}

		private float GetRandom()
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