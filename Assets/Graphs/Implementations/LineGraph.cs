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
			for (var i = 0; i < m_values.Count - 1; i++)
			{
				var currentValue = ToTexturePosition(m_values[i]);
				var nextValue = ToTexturePosition(m_values[i + 1]);

				var currentPoint = new Vector2(i, currentValue);
				var nextPoint = new Vector2(i + 1, nextValue);

				var delta = nextPoint - currentPoint;
				var normalizedDelta = delta.normalized;

				var currentPixel = currentPoint;
				
				if (normalizedDelta.x > normalizedDelta.y)
				{
					do
					{
						texture.SetPixel((int) currentPixel.x, (int) currentPixel.y, graphColor);
						currentPixel += normalizedDelta;
					} while (nextPoint.x - currentPixel.x > 0);
				}
				else if(normalizedDelta.y < 0)
				{
					do
					{
						texture.SetPixel((int) currentPixel.x, (int) currentPixel.y, graphColor);
						currentPixel += normalizedDelta;
					} while (nextPoint.y - currentPixel.y < 0);
				}
				else
				{
					do
					{
						texture.SetPixel((int) currentPixel.x, (int) currentPixel.y, graphColor);
						currentPixel += normalizedDelta;
					} while (nextPoint.y - currentPixel.y > 0);
				}
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
			if(!m_autoScale)
				return;

			if (value > m_maxScale)
			{
				m_maxScale = value;
			}
			else if (value < m_minScale)
			{
				m_minScale = value;
			}
			

			RecalculateDensity();
		}

		private void RecalculateDensity()
		{
			var fullRange = m_maxScale - m_minScale;
			m_verticalDensity = m_dimensions.y / fullRange;
		}

		private int ToTexturePosition(float value)
		{
			var offsetFromMinimum = value - m_minScale;
			return Mathf.FloorToInt(offsetFromMinimum * m_verticalDensity);
		}
	}
}