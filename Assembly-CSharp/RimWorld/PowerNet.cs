﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using Verse;

namespace RimWorld
{
	public class PowerNet
	{
		public PowerNetManager powerNetManager;

		public bool hasPowerSource;

		public List<CompPower> connectors = new List<CompPower>();

		public List<CompPower> transmitters = new List<CompPower>();

		public List<CompPowerTrader> powerComps = new List<CompPowerTrader>();

		public List<CompPowerBattery> batteryComps = new List<CompPowerBattery>();

		private float debugLastCreatedEnergy;

		private float debugLastRawStoredEnergy;

		private float debugLastApparentStoredEnergy;

		private const int MaxRestartTryInterval = 200;

		private const int MinRestartTryInterval = 30;

		private const float RestartMinFraction = 0.05f;

		private const int ShutdownInterval = 20;

		private const float ShutdownMinFraction = 0.05f;

		private const float MinStoredEnergyToTurnOn = 5f;

		private static List<CompPowerTrader> partsWantingPowerOn = new List<CompPowerTrader>();

		private static List<CompPowerTrader> potentialShutdownParts = new List<CompPowerTrader>();

		private List<CompPowerBattery> givingBats = new List<CompPowerBattery>();

		private static List<CompPowerBattery> batteriesShuffled = new List<CompPowerBattery>();

		public PowerNet(IEnumerable<CompPower> newTransmitters)
		{
			foreach (CompPower compPower in newTransmitters)
			{
				this.transmitters.Add(compPower);
				compPower.transNet = this;
				this.RegisterAllComponentsOf(compPower.parent);
				if (compPower.connectChildren != null)
				{
					List<CompPower> connectChildren = compPower.connectChildren;
					for (int i = 0; i < connectChildren.Count; i++)
					{
						this.RegisterConnector(connectChildren[i]);
					}
				}
			}
			this.hasPowerSource = false;
			for (int j = 0; j < this.transmitters.Count; j++)
			{
				if (this.IsPowerSource(this.transmitters[j]))
				{
					this.hasPowerSource = true;
					break;
				}
			}
		}

		public Map Map
		{
			get
			{
				return this.powerNetManager.map;
			}
		}

		public bool HasActivePowerSource
		{
			get
			{
				bool result;
				if (!this.hasPowerSource)
				{
					result = false;
				}
				else
				{
					for (int i = 0; i < this.transmitters.Count; i++)
					{
						if (this.IsActivePowerSource(this.transmitters[i]))
						{
							return true;
						}
					}
					result = false;
				}
				return result;
			}
		}

		private bool IsPowerSource(CompPower cp)
		{
			return cp is CompPowerBattery || (cp is CompPowerTrader && cp.Props.basePowerConsumption < 0f);
		}

		private bool IsActivePowerSource(CompPower cp)
		{
			CompPowerBattery compPowerBattery = cp as CompPowerBattery;
			bool result;
			if (compPowerBattery != null && compPowerBattery.StoredEnergy > 0f)
			{
				result = true;
			}
			else
			{
				CompPowerTrader compPowerTrader = cp as CompPowerTrader;
				result = (compPowerTrader != null && compPowerTrader.PowerOutput > 0f);
			}
			return result;
		}

		public void RegisterConnector(CompPower b)
		{
			if (this.connectors.Contains(b))
			{
				Log.Error("PowerNet registered connector it already had: " + b, false);
			}
			else
			{
				this.connectors.Add(b);
				this.RegisterAllComponentsOf(b.parent);
			}
		}

		public void DeregisterConnector(CompPower b)
		{
			this.connectors.Remove(b);
			this.DeregisterAllComponentsOf(b.parent);
		}

		private void RegisterAllComponentsOf(ThingWithComps parentThing)
		{
			CompPowerTrader comp = parentThing.GetComp<CompPowerTrader>();
			if (comp != null)
			{
				if (this.powerComps.Contains(comp))
				{
					Log.Error("PowerNet adding powerComp " + comp + " which it already has.", false);
				}
				else
				{
					this.powerComps.Add(comp);
				}
			}
			CompPowerBattery comp2 = parentThing.GetComp<CompPowerBattery>();
			if (comp2 != null)
			{
				if (this.batteryComps.Contains(comp2))
				{
					Log.Error("PowerNet adding batteryComp " + comp2 + " which it already has.", false);
				}
				else
				{
					this.batteryComps.Add(comp2);
				}
			}
		}

		private void DeregisterAllComponentsOf(ThingWithComps parentThing)
		{
			CompPowerTrader comp = parentThing.GetComp<CompPowerTrader>();
			if (comp != null)
			{
				this.powerComps.Remove(comp);
			}
			CompPowerBattery comp2 = parentThing.GetComp<CompPowerBattery>();
			if (comp2 != null)
			{
				this.batteryComps.Remove(comp2);
			}
		}

		public float CurrentEnergyGainRate()
		{
			float result;
			if (DebugSettings.unlimitedPower)
			{
				result = 100000f;
			}
			else
			{
				float num = 0f;
				for (int i = 0; i < this.powerComps.Count; i++)
				{
					if (this.powerComps[i].PowerOn)
					{
						num += this.powerComps[i].EnergyOutputPerTick;
					}
				}
				result = num;
			}
			return result;
		}

		public float CurrentStoredEnergy()
		{
			float num = 0f;
			for (int i = 0; i < this.batteryComps.Count; i++)
			{
				num += this.batteryComps[i].StoredEnergy;
			}
			return num;
		}

		public void PowerNetTick()
		{
			float num = this.CurrentEnergyGainRate();
			float num2 = this.CurrentStoredEnergy();
			if (num2 + num >= -1E-07f && !this.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare))
			{
				Profiler.BeginSample("PowerNetTick Excess Energy");
				float num3;
				if (this.batteryComps.Count > 0 && num2 >= 0.1f)
				{
					num3 = num2 - 5f;
				}
				else
				{
					num3 = num2;
				}
				if (UnityData.isDebugBuild)
				{
					this.debugLastApparentStoredEnergy = num3;
					this.debugLastCreatedEnergy = num;
					this.debugLastRawStoredEnergy = num2;
				}
				if (num3 + num >= 0f)
				{
					PowerNet.partsWantingPowerOn.Clear();
					for (int i = 0; i < this.powerComps.Count; i++)
					{
						if (!this.powerComps[i].PowerOn && FlickUtility.WantsToBeOn(this.powerComps[i].parent) && !this.powerComps[i].parent.IsBrokenDown())
						{
							PowerNet.partsWantingPowerOn.Add(this.powerComps[i]);
						}
					}
					if (PowerNet.partsWantingPowerOn.Count > 0)
					{
						int num4 = 200 / PowerNet.partsWantingPowerOn.Count;
						if (num4 < 30)
						{
							num4 = 30;
						}
						if (Find.TickManager.TicksGame % num4 == 0)
						{
							int num5 = Mathf.Max(1, Mathf.RoundToInt((float)PowerNet.partsWantingPowerOn.Count * 0.05f));
							for (int j = 0; j < num5; j++)
							{
								CompPowerTrader compPowerTrader = PowerNet.partsWantingPowerOn.RandomElement<CompPowerTrader>();
								if (!compPowerTrader.PowerOn)
								{
									if (num + num2 >= -(compPowerTrader.EnergyOutputPerTick + 1E-07f))
									{
										compPowerTrader.PowerOn = true;
										num += compPowerTrader.EnergyOutputPerTick;
									}
								}
							}
						}
					}
				}
				this.ChangeStoredEnergy(num);
				Profiler.EndSample();
			}
			else
			{
				Profiler.BeginSample("PowerNetTick Shutdown");
				if (Find.TickManager.TicksGame % 20 == 0)
				{
					PowerNet.potentialShutdownParts.Clear();
					for (int k = 0; k < this.powerComps.Count; k++)
					{
						if (this.powerComps[k].PowerOn && this.powerComps[k].EnergyOutputPerTick < 0f)
						{
							PowerNet.potentialShutdownParts.Add(this.powerComps[k]);
						}
					}
					if (PowerNet.potentialShutdownParts.Count > 0)
					{
						int num6 = Mathf.Max(1, Mathf.RoundToInt((float)PowerNet.potentialShutdownParts.Count * 0.05f));
						for (int l = 0; l < num6; l++)
						{
							PowerNet.potentialShutdownParts.RandomElement<CompPowerTrader>().PowerOn = false;
						}
					}
				}
				Profiler.EndSample();
			}
		}

		private void ChangeStoredEnergy(float extra)
		{
			if (extra > 0f)
			{
				this.DistributeEnergyAmongBatteries(extra);
			}
			else
			{
				float num = -extra;
				this.givingBats.Clear();
				for (int i = 0; i < this.batteryComps.Count; i++)
				{
					if (this.batteryComps[i].StoredEnergy > 1E-07f)
					{
						this.givingBats.Add(this.batteryComps[i]);
					}
				}
				float a = num / (float)this.givingBats.Count;
				int num2 = 0;
				while (num > 1E-07f)
				{
					for (int j = 0; j < this.givingBats.Count; j++)
					{
						float num3 = Mathf.Min(a, this.givingBats[j].StoredEnergy);
						this.givingBats[j].DrawPower(num3);
						num -= num3;
						if (num < 1E-07f)
						{
							return;
						}
					}
					num2++;
					if (num2 > 10)
					{
						break;
					}
				}
				if (num > 1E-07f)
				{
					Log.Warning("Drew energy from a PowerNet that didn't have it.", false);
				}
			}
		}

		private void DistributeEnergyAmongBatteries(float energy)
		{
			if (energy > 0f && this.batteryComps.Any<CompPowerBattery>())
			{
				PowerNet.batteriesShuffled.Clear();
				PowerNet.batteriesShuffled.AddRange(this.batteryComps);
				PowerNet.batteriesShuffled.Shuffle<CompPowerBattery>();
				int num = 0;
				for (;;)
				{
					num++;
					if (num > 10000)
					{
						break;
					}
					float num2 = float.MaxValue;
					for (int i = 0; i < PowerNet.batteriesShuffled.Count; i++)
					{
						num2 = Mathf.Min(num2, PowerNet.batteriesShuffled[i].AmountCanAccept);
					}
					if (energy < num2 * (float)PowerNet.batteriesShuffled.Count)
					{
						goto IL_139;
					}
					for (int j = PowerNet.batteriesShuffled.Count - 1; j >= 0; j--)
					{
						float amountCanAccept = PowerNet.batteriesShuffled[j].AmountCanAccept;
						bool flag = amountCanAccept <= 0f || amountCanAccept == num2;
						if (num2 > 0f)
						{
							PowerNet.batteriesShuffled[j].AddEnergy(num2);
							energy -= num2;
						}
						if (flag)
						{
							PowerNet.batteriesShuffled.RemoveAt(j);
						}
					}
					if (energy < 0.0005f || !PowerNet.batteriesShuffled.Any<CompPowerBattery>())
					{
						goto IL_1A3;
					}
				}
				Log.Error("Too many iterations.", false);
				goto IL_1AE;
				IL_139:
				float amount = energy / (float)PowerNet.batteriesShuffled.Count;
				for (int k = 0; k < PowerNet.batteriesShuffled.Count; k++)
				{
					PowerNet.batteriesShuffled[k].AddEnergy(amount);
				}
				energy = 0f;
				IL_1A3:
				IL_1AE:
				PowerNet.batteriesShuffled.Clear();
			}
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("POWERNET:");
			stringBuilder.AppendLine("  Created energy: " + this.debugLastCreatedEnergy);
			stringBuilder.AppendLine("  Raw stored energy: " + this.debugLastRawStoredEnergy);
			stringBuilder.AppendLine("  Apparent stored energy: " + this.debugLastApparentStoredEnergy);
			stringBuilder.AppendLine("  hasPowerSource: " + this.hasPowerSource);
			stringBuilder.AppendLine("  Connectors: ");
			foreach (CompPower compPower in this.connectors)
			{
				stringBuilder.AppendLine("      " + compPower.parent);
			}
			stringBuilder.AppendLine("  Transmitters: ");
			foreach (CompPower compPower2 in this.transmitters)
			{
				stringBuilder.AppendLine("      " + compPower2.parent);
			}
			stringBuilder.AppendLine("  powerComps: ");
			foreach (CompPowerTrader compPowerTrader in this.powerComps)
			{
				stringBuilder.AppendLine("      " + compPowerTrader.parent);
			}
			stringBuilder.AppendLine("  batteryComps: ");
			foreach (CompPowerBattery compPowerBattery in this.batteryComps)
			{
				stringBuilder.AppendLine("      " + compPowerBattery.parent);
			}
			return stringBuilder.ToString();
		}

		// Note: this type is marked as 'beforefieldinit'.
		static PowerNet()
		{
		}
	}
}
