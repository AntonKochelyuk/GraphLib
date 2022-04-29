using System;
using Graphs.Abstract;
using Graphs.Data;
using UnityEngine;
using JetBrains.Annotations;

namespace Graphs.Implementations
{
	public class LineGraph : IGraph
	{
		private readonly bool m_autoScale;
		
		private RingBuffer<float> m_values;
		private (int x, int y) m_dimensions;
		
		private float m_minScale;
		private float m_maxScale;
		
		private bool m_paused;
		
		private float m_verticalDensity;
		
		[CanBeNull]
		private Func<float> m_valueSource;

		public event Action<float> newValueReceivedEvent;

		public float minScale => m_minScale;
		public float maxScale => m_maxScale;

		public bool needsRedraw { get; private set; }
		
		public Color graphColor { get; }
		public string label { get; }

		public LineGraph(
			float minScale, 
			float maxScale, 
			bool autoScale, 
			Color lineColor,
			string graphLabel,
			[CanBeNull] Func<float> valueSource
		)
		{
			m_paused = false;

			m_minScale = minScale;
			m_maxScale = maxScale;
			
			m_autoScale = autoScale;
			graphColor = lineColor;

			label = graphLabel;
			
			m_valueSource = valueSource;
		}
		
		public void SetDimensions(int x, int y)
		{
			m_dimensions = (x, y);
			m_values = new RingBuffer<float>(x);
			
			RecalculateDensity();
		}

		public void SetScaleMin(float min)
		{
			m_minScale = min;

			RecalculateDensity();
		}

		public void SetScaleMax(float max)
		{
			m_maxScale = max;

			RecalculateDensity();
		}

		public void PushValue(float value)
		{
			if (m_paused)
			{
				return;
			}
			
			m_values.PushBack(value);
			newValueReceivedEvent?.Invoke(value);
			
			TryUpdateRange(value);
		}

		public void Update()
		{
			if (m_paused)
			{
				return;
			}

			Poll();
		}

		public void Populate(Texture2D texture)
		{
			if (m_values.Count < 2)
			{
				return;
			}

			var previousXPos = m_values.Count - 2;
			var currentXPos = m_values.Count - 1;

			DrawValuesAtTexture(texture, m_values[previousXPos], m_values[currentXPos], currentXPos);
		}

		public void Redraw(Texture2D texture)
		{
			needsRedraw = false;
			
			if (m_values.Count < 2)
			{
				return;
			}

			for (var i = 1; i < m_values.Count; i++)
			{
				var previous = m_values[i - 1];
				var current = m_values[i];
				
				DrawValuesAtTexture(texture, previous, current, i);
			}
		}

		private void DrawValuesAtTexture(Texture2D texture, float previousVal, float currentVal, int currentX)
		{
			var previousValTexturePos = ToTexturePosition(previousVal);
			var currentValTexturePos = ToTexturePosition(currentVal);
			
			var start = Mathf.Min(previousValTexturePos, currentValTexturePos);
			var end = Mathf.Max(previousValTexturePos, currentValTexturePos);
			
			for (var i = start; i <= end; i++)
			{
				texture.SetPixel(currentX, i, graphColor);
			}	
		}

		public void Pause()
		{
			m_paused = true;
		}

		public void Unpause()
		{
			m_paused = false;
		}

		public float GetValueAt(int index)
		{
			if (index < 0 || index >= m_values.Count)
			{
				return 0f;
			}

			return m_values[index];
		}

		private void Poll()
		{
			if (m_valueSource != null)
			{
				PushValue(m_valueSource.Invoke());
			}
		}
		
		private void TryUpdateRange(float value)
		{
			if (!m_autoScale)
				return;

			var scaleUpdated = false;
			
			if (value > m_maxScale)
			{
				scaleUpdated = true;
				m_maxScale = value;
			}
			else if (value < m_minScale)
			{
				scaleUpdated = true;
				m_minScale = value;
			}

			if (scaleUpdated)
			{
				needsRedraw = true;
				RecalculateDensity();
			}
		}

		private void RecalculateDensity()
		{
			var fullRange = m_maxScale - m_minScale;
			m_verticalDensity = m_dimensions.y / fullRange;
		}

		private int ToTexturePosition(float value)
		{
			var offsetFromMinimum = value - m_minScale;
			var yPos = Mathf.FloorToInt(offsetFromMinimum * m_verticalDensity);
			
			return yPos > m_dimensions.y - 1 ? m_dimensions.y - 1 : yPos;
		}
	}
}