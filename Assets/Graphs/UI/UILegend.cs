using System.Collections.Generic;
using Graphs.Abstract;
using UnityEngine;

namespace Graphs.UI
{
	public class UILegend : MonoBehaviour
	{
		[SerializeField]
		private UILegendEntry m_entryPrefab;

		[SerializeField]
		private Transform m_entriesHolder;

		private readonly Dictionary<IGraph, UILegendEntry> m_entries = new Dictionary<IGraph, UILegendEntry>();
		
		public void AddEntryForGraph(IGraph graph)
		{
			if (m_entries.ContainsKey(graph))
			{
				return;
			}
			
			m_entries.Add(graph, SpawnEntry(graph));
		}

		public void RemoveEntryForGraph(IGraph graph)
		{
			if (!m_entries.TryGetValue(graph, out var entry) || entry == null)
			{
				return;
			}

			graph.newValueReceivedEvent -= entry.SetValue;
			
			Destroy(entry.gameObject);
		}

		private UILegendEntry SpawnEntry(IGraph graph)
		{
			var entry = Instantiate(m_entryPrefab, m_entriesHolder);
			
			entry.SetColor(graph.graphColor);
			entry.SetLabel(graph.label);

			graph.newValueReceivedEvent += entry.SetValue;
			
			return entry;
		}
	}
}