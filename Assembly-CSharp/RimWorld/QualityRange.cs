using System;
using Verse;

namespace RimWorld
{
	public struct QualityRange : IEquatable<QualityRange>
	{
		public QualityCategory min;

		public QualityCategory max;

		public static QualityRange All
		{
			get
			{
				return new QualityRange(QualityCategory.Awful, QualityCategory.Legendary);
			}
		}

		public QualityRange(QualityCategory min, QualityCategory max)
		{
			this.min = min;
			this.max = max;
		}

		public bool Includes(QualityCategory p)
		{
			return (int)p >= (int)this.min && (int)p <= (int)this.max;
		}

		public static QualityRange FromString(string s)
		{
			string[] array = s.Split('~');
			return new QualityRange((QualityCategory)(byte)ParseHelper.FromString(array[0], typeof(QualityCategory)), (QualityCategory)(byte)ParseHelper.FromString(array[1], typeof(QualityCategory)));
		}

		public override string ToString()
		{
			return ((Enum)(object)this.min).ToString() + "~" + ((Enum)(object)this.max).ToString();
		}

		public override int GetHashCode()
		{
			return Gen.HashCombineStruct(((Enum)(object)this.min).GetHashCode(), this.max);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is QualityRange))
			{
				return false;
			}
			QualityRange qualityRange = (QualityRange)obj;
			return qualityRange.min == this.min && qualityRange.max == this.max;
		}

		public bool Equals(QualityRange other)
		{
			return other.min == this.min && other.max == this.max;
		}

		public static bool operator ==(QualityRange a, QualityRange b)
		{
			return a.min == b.min && a.max == b.max;
		}

		public static bool operator !=(QualityRange a, QualityRange b)
		{
			return !(a == b);
		}
	}
}
