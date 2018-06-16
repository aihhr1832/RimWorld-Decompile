﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	// Token: 0x020006AA RID: 1706
	public class Building_Grave : Building_Casket, IStoreSettingsParent, IAssignableBuilding, IHaulDestination
	{
		// Token: 0x1700057D RID: 1405
		// (get) Token: 0x06002472 RID: 9330 RVA: 0x00138018 File Offset: 0x00136418
		public override Graphic Graphic
		{
			get
			{
				Graphic graphic;
				if (this.HasCorpse)
				{
					if (this.def.building.fullGraveGraphicData == null)
					{
						graphic = base.Graphic;
					}
					else
					{
						if (this.cachedGraphicFull == null)
						{
							this.cachedGraphicFull = this.def.building.fullGraveGraphicData.GraphicColoredFor(this);
						}
						graphic = this.cachedGraphicFull;
					}
				}
				else
				{
					graphic = base.Graphic;
				}
				return graphic;
			}
		}

		// Token: 0x1700057E RID: 1406
		// (get) Token: 0x06002473 RID: 9331 RVA: 0x00138094 File Offset: 0x00136494
		public bool HasCorpse
		{
			get
			{
				return this.Corpse != null;
			}
		}

		// Token: 0x1700057F RID: 1407
		// (get) Token: 0x06002474 RID: 9332 RVA: 0x001380B8 File Offset: 0x001364B8
		public Corpse Corpse
		{
			get
			{
				for (int i = 0; i < this.innerContainer.Count; i++)
				{
					Corpse corpse = this.innerContainer[i] as Corpse;
					if (corpse != null)
					{
						return corpse;
					}
				}
				return null;
			}
		}

		// Token: 0x17000580 RID: 1408
		// (get) Token: 0x06002475 RID: 9333 RVA: 0x0013810C File Offset: 0x0013650C
		public IEnumerable<Pawn> AssigningCandidates
		{
			get
			{
				IEnumerable<Pawn> result;
				if (!base.Spawned)
				{
					result = Enumerable.Empty<Pawn>();
				}
				else
				{
					IEnumerable<Pawn> second = from Corpse x in base.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse)
					where x.InnerPawn.IsColonist
					select x.InnerPawn;
					result = base.Map.mapPawns.FreeColonistsSpawned.Concat(second);
				}
				return result;
			}
		}

		// Token: 0x17000581 RID: 1409
		// (get) Token: 0x06002476 RID: 9334 RVA: 0x001381A8 File Offset: 0x001365A8
		public IEnumerable<Pawn> AssignedPawns
		{
			get
			{
				if (this.assignedPawn != null)
				{
					yield return this.assignedPawn;
				}
				yield break;
			}
		}

		// Token: 0x17000582 RID: 1410
		// (get) Token: 0x06002477 RID: 9335 RVA: 0x001381D4 File Offset: 0x001365D4
		public int MaxAssignedPawnsCount
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x06002478 RID: 9336 RVA: 0x001381EA File Offset: 0x001365EA
		public void TryAssignPawn(Pawn pawn)
		{
			pawn.ownership.ClaimGrave(this);
		}

		// Token: 0x06002479 RID: 9337 RVA: 0x001381F9 File Offset: 0x001365F9
		public void TryUnassignPawn(Pawn pawn)
		{
			if (pawn == this.assignedPawn)
			{
				pawn.ownership.UnclaimGrave();
			}
		}

		// Token: 0x0600247A RID: 9338 RVA: 0x00138214 File Offset: 0x00136614
		public bool AssignedAnything(Pawn pawn)
		{
			return pawn.ownership.AssignedGrave != null;
		}

		// Token: 0x17000583 RID: 1411
		// (get) Token: 0x0600247B RID: 9339 RVA: 0x0013823C File Offset: 0x0013663C
		public bool StorageTabVisible
		{
			get
			{
				return this.assignedPawn == null && !this.HasCorpse;
			}
		}

		// Token: 0x0600247C RID: 9340 RVA: 0x00138268 File Offset: 0x00136668
		public StorageSettings GetStoreSettings()
		{
			return this.storageSettings;
		}

		// Token: 0x0600247D RID: 9341 RVA: 0x00138284 File Offset: 0x00136684
		public StorageSettings GetParentStoreSettings()
		{
			return this.def.building.fixedStorageSettings;
		}

		// Token: 0x0600247E RID: 9342 RVA: 0x001382AC File Offset: 0x001366AC
		public override void PostMake()
		{
			base.PostMake();
			this.storageSettings = new StorageSettings(this);
			if (this.def.building.defaultStorageSettings != null)
			{
				this.storageSettings.CopyFrom(this.def.building.defaultStorageSettings);
			}
		}

		// Token: 0x0600247F RID: 9343 RVA: 0x001382FC File Offset: 0x001366FC
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look<StorageSettings>(ref this.storageSettings, "storageSettings", new object[]
			{
				this
			});
		}

		// Token: 0x06002480 RID: 9344 RVA: 0x0013831F File Offset: 0x0013671F
		public override void EjectContents()
		{
			base.EjectContents();
			if (base.Spawned)
			{
				base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
			}
		}

		// Token: 0x06002481 RID: 9345 RVA: 0x0013834C File Offset: 0x0013674C
		public virtual void Notify_CorpseBuried(Pawn worker)
		{
			CompArt comp = base.GetComp<CompArt>();
			if (comp != null && !comp.Active)
			{
				comp.JustCreatedBy(worker);
				comp.InitializeArt(this.Corpse.InnerPawn);
			}
			base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
			worker.records.Increment(RecordDefOf.CorpsesBuried);
			TaleRecorder.RecordTale(TaleDefOf.BuriedCorpse, new object[]
			{
				worker,
				(this.Corpse == null) ? null : this.Corpse.InnerPawn
			});
		}

		// Token: 0x06002482 RID: 9346 RVA: 0x001383E8 File Offset: 0x001367E8
		public override bool Accepts(Thing thing)
		{
			bool result;
			if (!base.Accepts(thing))
			{
				result = false;
			}
			else if (this.HasCorpse)
			{
				result = false;
			}
			else
			{
				if (this.assignedPawn != null)
				{
					Corpse corpse = thing as Corpse;
					if (corpse == null)
					{
						return false;
					}
					if (corpse.InnerPawn != this.assignedPawn)
					{
						return false;
					}
				}
				else if (!this.storageSettings.AllowedToAccept(thing))
				{
					return false;
				}
				result = true;
			}
			return result;
		}

		// Token: 0x06002483 RID: 9347 RVA: 0x0013847C File Offset: 0x0013687C
		public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			bool result;
			if (base.TryAcceptThing(thing, allowSpecialEffects))
			{
				Corpse corpse = thing as Corpse;
				if (corpse != null && corpse.InnerPawn.ownership != null)
				{
					if (corpse.InnerPawn.ownership.AssignedGrave != this)
					{
						corpse.InnerPawn.ownership.UnclaimGrave();
					}
				}
				if (base.Spawned)
				{
					base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
				}
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06002484 RID: 9348 RVA: 0x00138510 File Offset: 0x00136910
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in this.<GetGizmos>__BaseCallProxy0())
			{
				yield return g;
			}
			if (this.StorageTabVisible)
			{
				foreach (Gizmo g2 in StorageSettingsClipboard.CopyPasteGizmosFor(this.storageSettings))
				{
					yield return g2;
				}
			}
			if (!this.HasCorpse)
			{
				yield return new Command_Action
				{
					defaultLabel = "CommandGraveAssignColonistLabel".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner", true),
					defaultDesc = "CommandGraveAssignColonistDesc".Translate(),
					action = delegate()
					{
						Find.WindowStack.Add(new Dialog_AssignBuildingOwner(this));
					},
					hotKey = KeyBindingDefOf.Misc3
				};
			}
			yield break;
		}

		// Token: 0x06002485 RID: 9349 RVA: 0x0013853C File Offset: 0x0013693C
		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (this.HasCorpse)
			{
				if (base.Tile != -1)
				{
					string text = GenDate.DateFullStringAt((long)GenDate.TickGameToAbs(this.Corpse.timeOfDeath), Find.WorldGrid.LongLatOf(base.Tile));
					stringBuilder.AppendLine();
					stringBuilder.Append("DiedOn".Translate(new object[]
					{
						text
					}));
				}
			}
			else if (this.assignedPawn != null)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("AssignedColonist".Translate());
				stringBuilder.Append(": ");
				stringBuilder.Append(this.assignedPawn.LabelCap);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x04001432 RID: 5170
		private StorageSettings storageSettings = null;

		// Token: 0x04001433 RID: 5171
		private Graphic cachedGraphicFull = null;

		// Token: 0x04001434 RID: 5172
		public Pawn assignedPawn = null;
	}
}
