using UnityEngine;

namespace Graphs.Abstract
{
	public interface IGraph : IReadOnlyGraph
	{
		void SetDimensions(int x, int y);

		void SetScaleMin(float min);
		void SetScaleMax(float max);
		
		void PushValue(float value);
		
		void Update();
		void Populate(Texture2D texture);
		
		void Pause();
		void Unpause();
	}
}
