﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	// Token: 0x02000FCD RID: 4045
	public class Triangulator
	{
		// Token: 0x060061C0 RID: 25024 RVA: 0x00314589 File Offset: 0x00312989
		public Triangulator(Vector2[] points)
		{
			this.m_points = new List<Vector2>(points);
		}

		// Token: 0x060061C1 RID: 25025 RVA: 0x003145AC File Offset: 0x003129AC
		public int[] Triangulate()
		{
			List<int> list = new List<int>();
			int count = this.m_points.Count;
			int[] result;
			if (count < 3)
			{
				result = list.ToArray();
			}
			else
			{
				int[] array = new int[count];
				if (this.Area() > 0f)
				{
					for (int i = 0; i < count; i++)
					{
						array[i] = i;
					}
				}
				else
				{
					for (int j = 0; j < count; j++)
					{
						array[j] = count - 1 - j;
					}
				}
				int k = count;
				int num = 2 * k;
				int num2 = 0;
				int num3 = k - 1;
				while (k > 2)
				{
					if (num-- <= 0)
					{
						return list.ToArray();
					}
					int num4 = num3;
					if (k <= num4)
					{
						num4 = 0;
					}
					num3 = num4 + 1;
					if (k <= num3)
					{
						num3 = 0;
					}
					int num5 = num3 + 1;
					if (k <= num5)
					{
						num5 = 0;
					}
					if (this.Snip(num4, num3, num5, k, array))
					{
						int item = array[num4];
						int item2 = array[num3];
						int item3 = array[num5];
						list.Add(item);
						list.Add(item2);
						list.Add(item3);
						num2++;
						int num6 = num3;
						for (int l = num3 + 1; l < k; l++)
						{
							array[num6] = array[l];
							num6++;
						}
						k--;
						num = 2 * k;
					}
				}
				list.Reverse();
				result = list.ToArray();
			}
			return result;
		}

		// Token: 0x060061C2 RID: 25026 RVA: 0x0031473C File Offset: 0x00312B3C
		private float Area()
		{
			int count = this.m_points.Count;
			float num = 0f;
			int index = count - 1;
			int i = 0;
			while (i < count)
			{
				Vector2 vector = this.m_points[index];
				Vector2 vector2 = this.m_points[i];
				num += vector.x * vector2.y - vector2.x * vector.y;
				index = i++;
			}
			return num * 0.5f;
		}

		// Token: 0x060061C3 RID: 25027 RVA: 0x003147C4 File Offset: 0x00312BC4
		private bool Snip(int u, int v, int w, int n, int[] V)
		{
			Vector2 a = this.m_points[V[u]];
			Vector2 b = this.m_points[V[v]];
			Vector2 c = this.m_points[V[w]];
			bool result;
			if (Mathf.Epsilon > (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x))
			{
				result = false;
			}
			else
			{
				for (int i = 0; i < n; i++)
				{
					if (i != u && i != v && i != w)
					{
						Vector2 p = this.m_points[V[i]];
						if (this.InsideTriangle(a, b, c, p))
						{
							return false;
						}
					}
				}
				result = true;
			}
			return result;
		}

		// Token: 0x060061C4 RID: 25028 RVA: 0x003148B8 File Offset: 0x00312CB8
		private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
		{
			float num = C.x - B.x;
			float num2 = C.y - B.y;
			float num3 = A.x - C.x;
			float num4 = A.y - C.y;
			float num5 = B.x - A.x;
			float num6 = B.y - A.y;
			float num7 = P.x - A.x;
			float num8 = P.y - A.y;
			float num9 = P.x - B.x;
			float num10 = P.y - B.y;
			float num11 = P.x - C.x;
			float num12 = P.y - C.y;
			float num13 = num * num10 - num2 * num9;
			float num14 = num5 * num8 - num6 * num7;
			float num15 = num3 * num12 - num4 * num11;
			return num13 >= 0f && num15 >= 0f && num14 >= 0f;
		}

		// Token: 0x04003FE5 RID: 16357
		private List<Vector2> m_points = new List<Vector2>();
	}
}
