using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Graphs.UI
{
	public class UIGraphMarker : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text m_text;

		[SerializeField]
		private Image m_image;

		public void SetValue(float value)
		{
			m_text.text = $"{value}";
		}

		public void SetColor(Color color)
		{
			m_text.color = color;
			m_image.color = color;
		}
	}
}