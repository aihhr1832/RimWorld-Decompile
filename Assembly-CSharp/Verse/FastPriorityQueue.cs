using System.Collections.Generic;

namespace Verse
{
	public class FastPriorityQueue<T>
	{
		protected List<T> innerList = new List<T>();

		protected IComparer<T> comparer;

		public int Count
		{
			get
			{
				return this.innerList.Count;
			}
		}

		public FastPriorityQueue()
		{
			this.comparer = Comparer<T>.Default;
		}

		public FastPriorityQueue(IComparer<T> comparer)
		{
			this.comparer = comparer;
		}

		public void Push(T item)
		{
			int num = this.innerList.Count;
			this.innerList.Add(item);
			while (num != 0)
			{
				int num2 = (num - 1) / 2;
				if (this.CompareElements(num, num2) >= 0)
					break;
				this.SwapElements(num, num2);
				num = num2;
			}
		}

		public T Pop()
		{
			T result = this.innerList[0];
			int num = 0;
			int count = this.innerList.Count;
			this.innerList[0] = this.innerList[count - 1];
			this.innerList.RemoveAt(count - 1);
			count = this.innerList.Count;
			while (true)
			{
				int num2 = num;
				int num3 = 2 * num + 1;
				int num4 = num3 + 1;
				if (num3 < count && this.CompareElements(num, num3) > 0)
				{
					num = num3;
				}
				if (num4 < count && this.CompareElements(num, num4) > 0)
				{
					num = num4;
				}
				if (num != num2)
				{
					this.SwapElements(num, num2);
					continue;
				}
				break;
			}
			return result;
		}

		public void Clear()
		{
			this.innerList.Clear();
		}

		protected void SwapElements(int i, int j)
		{
			T value = this.innerList[i];
			this.innerList[i] = this.innerList[j];
			this.innerList[j] = value;
		}

		protected int CompareElements(int i, int j)
		{
			return this.comparer.Compare(this.innerList[i], this.innerList[j]);
		}
	}
}
