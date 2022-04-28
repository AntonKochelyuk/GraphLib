using System;
using System.Collections;
using System.Collections.Generic;

namespace Graphs.Data
{
	public class RingBuffer<T> : IEnumerable<T>
	{
		private readonly ArraySegment<T> m_emptySegment = new ArraySegment<T>(new T[0]);
		
		private T[] m_buffer;

		private int m_start;
		private int m_end;
		
		private int m_count;

		private bool IsEmpty => m_count == 0;
		private bool IsFull => m_count == Capacity;
		
		public int Capacity => m_buffer.Length;
		
		public int Count => m_count;

		public T this[int index]
		{
			get
			{
				if (index >= m_count)
				{
					throw new IndexOutOfRangeException($"Cannot access index {index}, buffer size is {m_count}");
				}

				var ringIndex = ToInternalIndex(index);
				return m_buffer[ringIndex];
			}
		}

		public RingBuffer(int capacity)
		{
			if (capacity < 1)
			{
				throw new System.ArgumentException("Buffer size should be greater than 0");
			}

			m_buffer = new T[capacity];
		}
		
		public void PushBack(T value)
		{
			m_buffer[m_end] = value;
			m_end = (m_end + 1) % Capacity;
			
			if (IsFull)
			{
				m_start = m_end;
			}
			else
			{
				m_count++;
			}
		}

		public void Clear()
		{
			m_start = 0;
			m_end = 0;
			m_count = 0;
			
			Array.Clear(m_buffer, 0, m_buffer.Length);
		}

		public IEnumerator<T> GetEnumerator()
		{
			var segment1 = ArrayOne();
			var segment2 = ArrayTwo();

			for (var i = 0; i < segment1.Count; i++)
			{
				yield return segment1.Array![segment1.Offset + i];
			}

			for (var i = 0; i < segment2.Count; i++)
			{
				yield return segment2.Array![segment2.Offset + i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private int ToInternalIndex(int index)
		{
			return (m_start + index) % Capacity;
		}

		private ArraySegment<T> ArrayOne()
		{
			if (IsEmpty)
			{
				return m_emptySegment;
			}

			if (m_start < m_end)
			{
				return new ArraySegment<T>(m_buffer, m_start, m_end - m_start);
			}

			return new ArraySegment<T>(m_buffer, m_start, m_buffer.Length - m_start);
		}

		private ArraySegment<T> ArrayTwo()
		{
			if (IsEmpty)
			{
				return m_emptySegment;
			}

			if (m_start < m_end)
			{
				return new ArraySegment<T>(m_buffer, m_end, 0);
			}

			return new ArraySegment<T>(m_buffer, 0, m_end);
		}
	}
}