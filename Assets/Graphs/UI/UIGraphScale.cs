using System.Collections.Generic;
using Graphs.Abstract;
using TMPro;
using UnityEngine;

namespace Graphs.UI
{
	public class UIGraphScale : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text m_textPrefab;

		[SerializeField]
		private Transform m_minScaleParent;

		[SerializeField]
		private Transform m_maxScaleParent;

		private readonly Dictionary<IReadOnlyGraph, Texts> m_scaleTexts = new Dictionary<IReadOnlyGraph, Texts>();
		
		public void AddScaleTextForGraph(IReadOnlyGraph graph)
		{
			if (m_scaleTexts.ContainsKey(graph))
			{
				return;
			}

			var minText = SpawnText(m_minScaleParent, graph.graphColor);
			var maxText = SpawnText(m_maxScaleParent, graph.graphColor);
			
			m_scaleTexts.Add(graph, new Texts { minScaleText = minText, maxScaleText = maxText });
		}

		public void RemoveScaleTextForGraph(IReadOnlyGraph graph)
		{
			if (!m_scaleTexts.TryGetValue(graph, out var texts))
			{
				return;
			}

			if (texts.maxScaleText)
			{
				Destroy(texts.maxScaleText);
			}

			if (texts.minScaleText)
			{
				Destroy(texts.minScaleText);
			}

			m_scaleTexts.Remove(graph);
		}

		public void UpdateScaleFor(IReadOnlyGraph graph)
		{
			if (!m_scaleTexts.TryGetValue(graph, out var texts))
			{
				return;
			}

			texts.maxScaleText.text = $"{graph.maxScale:0.0}";
			texts.minScaleText.text = $"{graph.minScale:0.0}";
		}

		private TMP_Text SpawnText(Transform parent, Color textColor)
		{
			var text = Instantiate(m_textPrefab, parent);
			text.gameObject.SetActive(true);

			text.color = textColor;
			return text;
		}

		private struct Texts
		{
			public TMP_Text minScaleText;
			public TMP_Text maxScaleText;
		}
	}
}