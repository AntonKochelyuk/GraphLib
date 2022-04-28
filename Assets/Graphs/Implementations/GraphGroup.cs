using System;
using System.Collections.Generic;
using Graphs.Abstract;
using Graphs.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Graphs.Implementations
{
	public class GraphGroup : MonoBehaviour, IGraphGroup
	{
		[SerializeField]
		private Vector2 m_tooltipOffset;
		
		[SerializeField]
		private RawImage m_textureTarget;
		
		[SerializeField]
		private UILegend m_legend;
		
		[SerializeField]
		private UIGraphMarker m_markerPrefab;

		[SerializeField]
		private UIGraphScale m_verticalScale;
		
		[SerializeField]
		private TMP_Text m_title;

		[SerializeField]
		private UIValuesTooltip m_valuesTooltip;

		[SerializeField]
		private int m_capacity = 256;
		
		private readonly List<IGraph> m_graphs = new List<IGraph>();
		private readonly List<MarkerSettings> m_markers = new List<MarkerSettings>();
		
		private bool m_graphsPaused;
		private bool m_scaleChanged;
		
		private RectTransform m_rectTransform;
		private Texture2D m_texture;

		private bool m_scaleSyncEnabled;
		private float m_minScaleSync = int.MaxValue;
		private float m_maxScaleSync = int.MinValue;
		
		private float m_verticalDensity;
		
		private Color[] m_clearColorArray;
		
		private Rect m_screenRect;
		private Rect m_textureRect;

		private void Awake()
		{
			m_scaleChanged = true;
			m_rectTransform = transform as RectTransform;
			
			m_screenRect = GetScreenRect(m_rectTransform);
			m_textureRect = GetScreenRect(m_textureTarget.rectTransform);
			
			m_texture = CreateTexture();
			m_clearColorArray = new Color[m_texture.width * m_texture.height];
			for (var i = 0; i < m_clearColorArray.Length; i++)
			{
				m_clearColorArray[i] = new Color(.3f, .3f, .3f, .8f);
			}
			
			ClearTexture();
			
			m_textureTarget.texture = m_texture;
		}

		private void OnDestroy()
		{
			Destroy(m_texture);
		}

		public string title { get; private set; }

		public IDisposable AddGraph(IGraph graph)
		{
			if (m_graphs.Contains(graph))
			{
				return null;
			}

			graph.SetDimensions(m_capacity, m_texture.height);
			
			m_graphs.Add(graph);
			m_legend.AddEntryForGraph(graph);
			m_valuesTooltip.AddTextForGraph(graph);
			m_verticalScale.AddScaleTextForGraph(graph);

			return new DisposableAction(() => RemoveGraph(graph));
		}

		public void SetSyncMode(bool syncEnabled)
		{
			m_scaleSyncEnabled = syncEnabled;
		}

		public void ClearMarkers()
		{
			m_markers.Clear();
		}

		public IGraphGroup AddMarkerAt(float value, Color markerColor)
		{
			if (!m_scaleSyncEnabled)
			{
				Debug.LogWarning($"You are trying to create marker, but graphs scale synchronisation is disabled. " +
				                 $"Markers would not work as expected. To fix the error, enable scale synchronisation with " +
				                 $"{nameof(IGraphGroup)}.{nameof(SetSyncMode)}(true)");
			}

			foreach (var marker in m_markers)
			{
				if (Mathf.Approximately(marker.value, value))
				{
					return this;
				}
			}

			var instance = CreateMarkerAtValue(value);
			instance.SetValue(value);
			instance.SetColor(markerColor);
			
			var markerSettings = new MarkerSettings
			{
				value = value,
				color = markerColor,
				markerInstance = instance
			};

			m_markers.Add(markerSettings);
			return this;
		}

		private void RemoveGraph(IGraph graph)
		{
			if (!m_graphs.Contains(graph))
			{
				return;
			}
			
			m_verticalScale.RemoveScaleTextForGraph(graph);
			m_valuesTooltip.RemoveTextForGraph(graph);
			m_legend.RemoveEntryForGraph(graph);
			m_graphs.Remove(graph);
		}
		
		private UIGraphMarker CreateMarkerAtValue(float value)
		{
			var fullRange = Mathf.Abs(m_maxScaleSync - m_minScaleSync);
			var normalizedPosition = value / fullRange;

			var localPosition = m_screenRect.height * normalizedPosition;

			var markerInstance = Instantiate(m_markerPrefab, m_textureTarget.transform);
			markerInstance.transform.localPosition = new Vector3(0, localPosition, 0);

			return markerInstance;
		}

		public void SwitchGroupPausedState()
		{
			if (!m_graphsPaused)
			{
				foreach (var graph in m_graphs)
				{
					graph.Pause();
				}
			}
			else
			{
				foreach (var graph in m_graphs)
				{
					graph.Unpause();
				}
			}

			m_graphsPaused = !m_graphsPaused;
		}

		public void SetTitle(string groupTitle)
		{
			if (!string.IsNullOrEmpty(title))
			{
				Debug.LogError("Graph title already has been set, unable to set new title");
				return;
			}

			title = groupTitle;
			m_title.text = groupTitle;
		}

		private void LateUpdate()
		{
			ClearTexture();
			
			if (MouseInsideGroup())
			{
				if (Input.GetMouseButtonDown(0))
				{
					SwitchGroupPausedState();
				}
			}

			if (MouseInsideTexture())
			{
				ShowValuesTooltip();
			}
			else
			{
				HideValuesPopup();
			}

			UpdateGraphs();
			
			if (m_scaleChanged && m_scaleSyncEnabled)
			{
				UpdateMarkersPositions();
				m_scaleChanged = false;
			}
			
			UpdateScaleUI();
			
			m_texture.Apply();
		}

		private void UpdateScaleUI()
		{
			foreach (var graph in m_graphs)
			{
				m_verticalScale.UpdateScaleFor(graph);
			}
		}

		private void UpdateMarkersPositions()
		{
			foreach (var marker in m_markers)
			{
				var offsetFromMinimum = marker.value - m_minScaleSync;
				var verticalDensity = m_texture.height / (m_maxScaleSync - m_minScaleSync);
				
				var texturePosition = Mathf.FloorToInt(offsetFromMinimum * verticalDensity);

				if (texturePosition > m_texture.height)
				{
					marker.markerInstance.gameObject.SetActive(false);
					continue;
				}

				if (!marker.markerInstance.gameObject.activeInHierarchy)
				{
					marker.markerInstance.gameObject.SetActive(true);
				}
				
				marker.rectTransform.anchoredPosition = new Vector3(0, texturePosition, 0);
			}
		}
		
		private void UpdateGraphs()
		{
			foreach (var graph in m_graphs)
			{
				if (!m_graphsPaused)
				{
					graph.Update();
				}

				if (m_scaleSyncEnabled)
				{
					TryUpdateScaleFrom(graph);
				}
				
				graph.Populate(m_texture);
			}
		}

		private void TryUpdateScaleFrom(IGraph graph)
		{
			if (graph.minScale > m_minScaleSync)
			{
				graph.SetScaleMin(m_minScaleSync);
			}
			else
			{
				m_scaleChanged = true;
				m_minScaleSync = graph.minScale;
			}

			if (graph.maxScale < m_maxScaleSync)
			{
				graph.SetScaleMax(m_maxScaleSync);
			}
			else
			{
				m_scaleChanged = true;
				m_maxScaleSync = graph.maxScale;
			}
		}

		private bool MouseInsideGroup()
		{
			var mousePosition = (Vector2) Input.mousePosition;
			return m_screenRect.Contains(mousePosition);
		}

		private bool MouseInsideTexture()
		{
			var mousePosition = (Vector2) Input.mousePosition;
			return m_textureRect.Contains(mousePosition);
		}

		private void ShowValuesTooltip()
		{
			var mousePosition = (Vector2) Input.mousePosition;
			var horizontalDensity = m_texture.width / m_textureRect.width;
			
			var selectedPixel = (int) ((mousePosition.x - m_textureRect.x) * horizontalDensity);

			for (var i = 0; i < m_texture.height; i++)
			{
				m_texture.SetPixel(selectedPixel, i, Color.white);
			}

			foreach (var graph in m_graphs)
			{
				m_valuesTooltip.SetTextForGraph(graph, graph.GetValueAt(selectedPixel));
			}

			if (!m_valuesTooltip.gameObject.activeSelf)
			{
				m_valuesTooltip.gameObject.SetActive(true);
			}

			m_valuesTooltip.transform.position = mousePosition + m_tooltipOffset;
		}

		private void HideValuesPopup()
		{
			if (m_valuesTooltip.gameObject.activeSelf)
			{
				m_valuesTooltip.gameObject.SetActive(false);
			}
		}
		
		private Texture2D CreateTexture()
		{
			if (m_texture != null)
			{
				return m_texture;
			}

			var rect = GetScreenRect(m_textureTarget.rectTransform);
			var texture = new Texture2D(m_capacity, (int) rect.height);

			texture.filterMode = FilterMode.Point;
			return texture;
		}

		private Rect GetScreenRect(RectTransform rectTransform)
		{
			var corners = new Vector3[4];
			rectTransform.GetWorldCorners(corners);
			
			var bottomLeft = corners[0];
			var topRight = corners[2];
			
			var width = topRight.x - bottomLeft.x;
			var height = topRight.y - bottomLeft.y;

			return new Rect(bottomLeft.x, bottomLeft.y, width, height);
		}
		
		private void ClearTexture()
		{
			m_texture.SetPixels(m_clearColorArray);
			m_texture.Apply();
		}
		
		private struct MarkerSettings
		{
			private RectTransform m_rectTransform;

			
			public float value { get; set; }
			public Color color { get; set; }

			public UIGraphMarker markerInstance { get; set; }
			public RectTransform rectTransform
			{
				get
				{
					if (m_rectTransform == null)
					{
						m_rectTransform = markerInstance == null ? null : markerInstance.transform as RectTransform;
					}

					return m_rectTransform;
				}
			}
		}
	}
}