using UnityEngine;

namespace Graphs.Abstract
{
	public interface IGraph : IReadOnlyGraph
	{
		void SetDimensions(int x, int y);

		void SetScaleMin(float min);
		void SetScaleMax(float max);

		void Update();
		
		void PushValue(float value);
		
		void Populate(Texture2D texture);
		void Redraw(Texture2D texture);
		
		void Pause();
		void Unpause();
	}
}
