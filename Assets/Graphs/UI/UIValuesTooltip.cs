using System.Collections.Generic;
using Graphs.Abstract;
using TMPro;
using UnityEngine;

namespace Graphs.UI
{
	public class UIValuesTooltip : MonoBehaviour
	{
		[SerializeField]
		private Transform m_valuesParent;

		[SerializeField]
		private TMP_Text m_textPrefab;

		private readonly Dictionary<IReadOnlyGraph, TMP_Text> m_textInstances = new Dictionary<IReadOnlyGraph, TMP_Text>();

		private void Awake()
		{
			m_textPrefab.gameObject.SetActive(false);
		}

		public void AddTextForGraph(IReadOnlyGraph graph)
		{
			if (m_textInstances.ContainsKey(graph))
			{
				return;
			}

			var text = SpawnText(graph.graphColor);
			m_textInstances.Add(graph, text);
		}
		
		public void RemoveTextForGraph(IReadOnlyGraph graph)
		{
			if (!m_textInstances.TryGetValue(graph, out var text))
			{
				return;
			}

			if (text && text.gameObject)
			{
				Destroy(text.gameObject);
			}
			
			m_textInstances.Remove(graph);
		}

		public void SetTextForGraph(IReadOnlyGraph graph, float value)
		{
			if (!m_textInstances.TryGetValue(graph, out var text))
			{
				return;
			}

			text.text = $"{value:0.000}";
		}

		private TMP_Text SpawnText(Color color)
		{
			var text = Instantiate(m_textPrefab, m_valuesParent);
			
			text.gameObject.SetActive(true);
			text.color = color;
			
			return text;
		}
	}
}