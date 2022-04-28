using System;
using Graphs.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Graphs.Implementations
{
	public class LineGraphBuilder
	{
		private bool m_overrideColor;
		private Color m_color;
		
		private Func<float> m_valuesGetter;
		
		private string m_label = "Graph";
		private float m_scaleMin = -5;
		private float m_scaleMax = 5;
		
		private bool m_autoScale = true;
		
		public static LineGraphBuilder Create()
		{
			return new LineGraphBuilder();
		}

		public static LineGraphBuilder FromGraphData(GraphData data)
		{
			return LineGraphBuilder.Create()
								   .WithColor(data.color)
								   .WithCustomScale(data.scaleMin, data.scaleMax)
								   .WithAutoScaleMode(data.autoScale)
								   .WithLabel(data.label);
		}
		
		private LineGraphBuilder()
		{
		}
		
		public LineGraphBuilder WithValuesGetter(Func<float> valuesGetter)
		{
			m_valuesGetter = valuesGetter;
			return this;
		}

		public LineGraphBuilder WithColor(Color color)
		{
			m_overrideColor = true;
			m_color = color;

			return this;
		}

		public LineGraphBuilder WithCustomScale(float scaleMin, float scaleMax)
		{
			m_scaleMin = scaleMin;
			m_scaleMax = scaleMax;
			
			return this;
		}

		public LineGraphBuilder WithAutoScaleMode(bool autoScaleEnabled)
		{
			m_autoScale = autoScaleEnabled;
			return this;
		}

		public LineGraphBuilder WithLabel(string label)
		{
			m_label = label;
			return this;
		}

		public LineGraph Build()
		{
			var color = m_overrideColor ? m_color : GetRandomColor();

			return new LineGraph(
				m_scaleMin, 
				m_scaleMax, 
				m_autoScale, 
				color, 
				m_label,
				m_valuesGetter
			);
		}
		
		private Color GetRandomColor()
		{
			return new Color(Random.value, Random.value, Random.value, 1f);
		}
	}
}