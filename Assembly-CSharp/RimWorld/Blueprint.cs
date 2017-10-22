using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class Blueprint : ThingWithComps, IConstructible
	{
		private static List<Thing> tmpAdjacentThings = new List<Thing>();

		public override string Label
		{
			get
			{
				return base.def.entityDefToBuild.label + "BlueprintLabelExtra".Translate();
			}
		}

		protected abstract float WorkTotal
		{
			get;
		}

		public override void Tick()
		{
			base.Tick();
			if (!GenConstruct.CanBuildOnTerrain(base.def.entityDefToBuild, base.Position, base.Map, base.Rotation, null))
			{
				this.Destroy(DestroyMode.Cancel);
			}
		}

		public override void Draw()
		{
			if (base.def.drawerType == DrawerType.RealtimeOnly)
			{
				base.Draw();
			}
			else
			{
				base.Comps_PostDraw();
			}
		}

		public virtual bool TryReplaceWithSolidThing(Pawn workerPawn, out Thing createdThing, out bool jobEnded)
		{
			jobEnded = false;
			if (this.FirstBlockingThing(workerPawn, null, false) != null)
			{
				workerPawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
				jobEnded = true;
				createdThing = null;
				return false;
			}
			Thing thing = this.FirstBlockingThing(null, null, false);
			if (thing != null)
			{
				Log.Error(workerPawn + " tried to replace blueprint " + this.ToString() + " at " + base.Position + " with solid thing, but it is blocked by " + thing + " at " + thing.Position);
				if (thing != workerPawn)
				{
					createdThing = null;
					return false;
				}
			}
			createdThing = this.MakeSolidThing();
			Map map = base.Map;
			GenAdjFast.AdjacentThings8Way(this, Blueprint.tmpAdjacentThings);
			GenSpawn.WipeExistingThings(base.Position, base.Rotation, createdThing.def, map, DestroyMode.Deconstruct);
			if (!base.Destroyed)
			{
				this.Destroy(DestroyMode.Vanish);
			}
			createdThing.SetFactionDirect(workerPawn.Faction);
			GenSpawn.Spawn(createdThing, base.Position, map, base.Rotation, false);
			for (int i = 0; i < Blueprint.tmpAdjacentThings.Count; i++)
			{
				Building_CrashedShipPart building_CrashedShipPart = Blueprint.tmpAdjacentThings[i] as Building_CrashedShipPart;
				if (building_CrashedShipPart != null)
				{
					building_CrashedShipPart.Notify_AdjacentBlueprintReplacedWithSolidThing(workerPawn);
				}
			}
			Blueprint.tmpAdjacentThings.Clear();
			return true;
		}

		protected abstract Thing MakeSolidThing();

		public abstract List<ThingCountClass> MaterialsNeeded();

		public abstract ThingDef UIStuff();

		public Thing BlockingHaulableOnTop()
		{
			if (base.def.entityDefToBuild.passability == Traversability.Standable)
			{
				return null;
			}
			CellRect.CellRectIterator iterator = this.OccupiedRect().GetIterator();
			while (!iterator.Done())
			{
				List<Thing> thingList = iterator.Current.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (thing.def.EverHaulable)
					{
						return thing;
					}
				}
				iterator.MoveNext();
			}
			return null;
		}

		public Thing FirstBlockingThing(Pawn pawnToIgnore = null, Thing thingToIgnore = null, bool haulableOnly = false)
		{
			CellRect.CellRectIterator iterator = this.OccupiedRect().GetIterator();
			while (!iterator.Done())
			{
				List<Thing> thingList = iterator.Current.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if ((!haulableOnly || thing.def.EverHaulable) && GenConstruct.BlocksFramePlacement(this, thing) && thing != pawnToIgnore && thing != thingToIgnore)
					{
						return thing;
					}
				}
				iterator.MoveNext();
			}
			return null;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			stringBuilder.Append("WorkLeft".Translate() + ": " + this.WorkTotal.ToStringWorkAmount());
			return stringBuilder.ToString();
		}
	}
}
