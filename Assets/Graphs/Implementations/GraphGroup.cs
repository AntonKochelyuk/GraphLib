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
		private TMP_Text m_title;

		[SerializeField]
		private RectTransform m_verticalMarker;
		
		[Space]
		
		[SerializeField]
		private UILegend m_legend;
		
		[SerializeField]
		private UIGraphMarker m_markerPrefab;

		[SerializeField]
		private UIGraphScale m_verticalScale;
		
		[SerializeField]
		private UIValuesTooltip m_valuesTooltip;

		[SerializeField]
		private int m_capacity = 256;
		
		private readonly List<IGraph> m_graphs = new List<IGraph>();
		private readonly List<MarkerSettings> m_markers = new List<MarkerSettings>();
		
		private bool m_graphsPaused;
		private bool m_scaleChanged;
		
		private RectTransform m_rectTransform;

		private int m_textureWidth;
		private int m_textureHeight;

		private int m_currentTextureIndex;
		private Texture2D[] m_textures = new Texture2D[2];
		
		private bool m_scaleSyncEnabled;
		private float m_minScaleSync = int.MaxValue;
		private float m_maxScaleSync = int.MinValue;
		
		private float m_verticalDensity;
		private float m_horizontalDensity;

		private Color m_clearColor;
		private Color[] m_clearColorArray;
		
		private Rect m_screenRect;
		private Rect m_textureRect;
		private Vector3 m_verticalMarkerPosition;

		private int m_updateCount = 0;
		
		public string title { get; private set; }
		
		private void Awake()
		{
			m_scaleChanged = true;
			m_rectTransform = transform as RectTransform;

			m_verticalMarkerPosition = m_textureTarget.transform.position;
			
			m_screenRect = GetScreenRect(m_rectTransform);
			m_textureRect = GetScreenRect(m_textureTarget.rectTransform);

			m_textures[0] = CreateTexture();
			m_textures[1] = CreateTexture();

			m_textureWidth = m_textures[0].width;
			m_textureHeight = m_textures[0].height;

			m_horizontalDensity = m_textureWidth / m_textureRect.width;
			m_verticalDensity = m_textureHeight / (m_maxScaleSync - m_minScaleSync);

			m_clearColor = new Color(.3f, .3f, .3f, .8f);
			m_clearColorArray = new Color[m_textureWidth * m_textureHeight];
			for (var i = 0; i < m_clearColorArray.Length; i++)
			{
				m_clearColorArray[i] = m_clearColor;
			}
			
			ClearTextures();
			
			m_textureTarget.texture = m_textures[0];
		}

		private void ClearTextures()
		{
			m_textures[0].SetPixels(m_clearColorArray);
			m_textures[1].SetPixels(m_clearColorArray);
			
			m_textures[0].Apply();
			m_textures[1].Apply();
		}

		private void OnDestroy()
		{
			Destroy(m_textures[0]);
			Destroy(m_textures[1]);
		}
		
		public void AddGraph(IGraph graph)
		{
			if (m_graphs.Contains(graph))
			{
				return;
			}

			graph.SetDimensions(m_capacity, m_textureHeight);
			
			m_graphs.Add(graph);
			m_legend.AddEntryForGraph(graph);
			m_valuesTooltip.AddTextForGraph(graph);
			m_verticalScale.AddScaleTextForGraph(graph);
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

		public void RemoveGraph(IGraph graph)
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
			if (m_graphs.Count == 0)
			{
				return;
			}

			HandleTooltip();

			if (m_graphsPaused)
			{
				return;
			}

			var currentTexture = m_textures[m_currentTextureIndex];
			m_currentTextureIndex = m_currentTextureIndex == 0 ? 1 : 0;

			var nextTexture = m_textures[m_currentTextureIndex];
			
			UpdateGraphs();

			var needsRedraw = false;

			foreach (var graph in m_graphs)
			{
				if (graph.needsRedraw)
				{
					needsRedraw = true;
					break;
				}
			}
			
			PrepareTextures(currentTexture, needsRedraw);
			PopulateTexture(currentTexture, needsRedraw);
			
			if (m_scaleChanged && m_scaleSyncEnabled)
			{
				UpdateMarkersPositions();
			}
			
			UpdateScaleUI();
			
			currentTexture.Apply();

			if (needsRedraw)
			{
				CopyOldValues(currentTexture, nextTexture, 0);
			}

			m_scaleChanged = false;
			m_updateCount++;
		}

		private void HandleTooltip()
		{
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
		}

		private void PrepareTextures(Texture2D currentTexture, bool needsRedraw)
		{
			if (needsRedraw)
			{
				ClearTextures();
				return;
			}

			if (m_updateCount > m_textureWidth)
			{
				CopyOldValues(m_textures[m_currentTextureIndex], currentTexture, 1);
				ClearLastColumn(currentTexture);
			}
			else
			{
				CopyOldValues(m_textures[m_currentTextureIndex], currentTexture, 0);
			}

		}

		private void CopyOldValues(Texture source, Texture destination, int startIndex)
		{
			Graphics.CopyTexture(source, 0, 0, startIndex, 0, 
				source.width - 1, source.height, destination, 0, 0, 0, 0);
		}

		private void ClearLastColumn(Texture2D texture)
		{
			var xPos = m_textureWidth - 1;
			
			for (var i = 0; i < texture.height; i++)
			{
				texture.SetPixel(xPos, i, m_clearColor);
			}
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
				
				var texturePosition = Mathf.FloorToInt(offsetFromMinimum * m_verticalDensity);

				if (texturePosition > m_textureHeight)
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
			}
		}

		private void PopulateTexture(Texture2D currentTexture, bool needsRedraw)
		{
			foreach (var graph in m_graphs)
			{
				if (needsRedraw)
				{
					graph.Redraw(currentTexture);
					continue;
				}

				graph.Populate(currentTexture);
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
			var selectedPixel = (int) ((mousePosition.x - m_textureRect.x) * m_horizontalDensity);
			
			foreach (var graph in m_graphs)
			{
				m_valuesTooltip.SetTextForGraph(graph, graph.GetValueAt(selectedPixel));
			}

			if (!m_valuesTooltip.gameObject.activeSelf)
			{
				m_valuesTooltip.gameObject.SetActive(true);
			}

			if (!m_verticalMarker.gameObject.activeSelf)
			{
				m_verticalMarker.gameObject.SetActive(true);
			}

			m_verticalMarkerPosition.x = mousePosition.x;
			
			m_valuesTooltip.transform.position = mousePosition + m_tooltipOffset;
			m_verticalMarker.transform.position = m_verticalMarkerPosition;
		}

		private void HideValuesPopup()
		{
			if (m_valuesTooltip.gameObject.activeSelf)
			{
				m_valuesTooltip.gameObject.SetActive(false);
			}

			if (m_verticalMarker.gameObject.activeSelf)
			{
				m_verticalMarker.gameObject.SetActive(false);
			}
		}
		
		private Texture2D CreateTexture()
		{
			var rect = GetScreenRect(m_textureTarget.rectTransform);
			var texture = new Texture2D(m_capacity, (int) rect.height);

			texture.filterMode = FilterMode.Bilinear;
			
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