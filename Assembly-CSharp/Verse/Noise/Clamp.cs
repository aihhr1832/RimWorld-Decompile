﻿using System;
using System.Diagnostics;

namespace Verse.Noise
{
	public class Clamp : ModuleBase
	{
		private double m_min = -1.0;

		private double m_max = 1.0;

		public Clamp() : base(1)
		{
		}

		public Clamp(ModuleBase input) : base(1)
		{
			this.modules[0] = input;
		}

		public Clamp(double min, double max, ModuleBase input) : base(1)
		{
			this.Minimum = min;
			this.Maximum = max;
			this.modules[0] = input;
		}

		public double Maximum
		{
			get
			{
				return this.m_max;
			}
			set
			{
				this.m_max = value;
			}
		}

		public double Minimum
		{
			get
			{
				return this.m_min;
			}
			set
			{
				this.m_min = value;
			}
		}

		public void SetBounds(double min, double max)
		{
			Debug.Assert(min < max);
			this.m_min = min;
			this.m_max = max;
		}

		public override double GetValue(double x, double y, double z)
		{
			Debug.Assert(this.modules[0] != null);
			if (this.m_min > this.m_max)
			{
				double min = this.m_min;
				this.m_min = this.m_max;
				this.m_max = min;
			}
			double value = this.modules[0].GetValue(x, y, z);
			double result;
			if (value < this.m_min)
			{
				result = this.m_min;
			}
			else if (value > this.m_max)
			{
				result = this.m_max;
			}
			else
			{
				result = value;
			}
			return result;
		}
	}
}
