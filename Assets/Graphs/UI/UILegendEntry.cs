using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Graphs.UI
{
	public class UILegendEntry : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text m_entryLabel;

		[SerializeField]
		private TMP_Text m_currentValue;
		
		[SerializeField]
		private Image m_colorSample;
		
		public void SetLabel(string text)
		{
			m_entryLabel.text = text;
		}

		public void SetColor(Color color)
		{
			m_colorSample.color = color;
		}

		public void SetValue(float value)
		{
			//TODO: string аллокация
			m_currentValue.text = $"{value:0.00}";
		}
	}
}