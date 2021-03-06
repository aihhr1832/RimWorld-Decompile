﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld.BaseGen
{
	public struct ResolveParams
	{
		public CellRect rect;

		public Faction faction;

		private Dictionary<string, object> custom;

		public int? ancientTempleEntranceHeight;

		public PawnGroupMakerParms pawnGroupMakerParams;

		public PawnGroupKindDef pawnGroupKindDef;

		public RoofDef roofDef;

		public bool? noRoof;

		public bool? addRoomCenterToRootsToUnfog;

		public Thing singleThingToSpawn;

		public ThingDef singleThingDef;

		public ThingDef singleThingStuff;

		public int? singleThingStackCount;

		public bool? skipSingleThingIfHasToWipeBuildingOrDoesntFit;

		public Pawn singlePawnToSpawn;

		public PawnKindDef singlePawnKindDef;

		public bool? disableSinglePawn;

		public Lord singlePawnLord;

		public Predicate<IntVec3> singlePawnSpawnCellExtraPredicate;

		public PawnGenerationRequest? singlePawnGenerationRequest;

		public Action<Thing> postThingSpawn;

		public Action<Thing> postThingGenerate;

		public int? mechanoidsCount;

		public int? hivesCount;

		public bool? disableHives;

		public Rot4? thingRot;

		public ThingDef wallStuff;

		public float? chanceToSkipWallBlock;

		public TerrainDef floorDef;

		public float? chanceToSkipFloor;

		public ThingDef filthDef;

		public FloatRange? filthDensity;

		public bool? clearEdificeOnly;

		public bool? clearFillageOnly;

		public bool? clearRoof;

		public int? ancientCryptosleepCasketGroupID;

		public PodContentsType? podContentsType;

		public ThingSetMakerDef thingSetMakerDef;

		public ThingSetMakerParams? thingSetMakerParams;

		public IList<Thing> stockpileConcreteContents;

		public float? stockpileMarketValue;

		public int? innerStockpileSize;

		public int? edgeDefenseWidth;

		public int? edgeDefenseTurretsCount;

		public int? edgeDefenseMortarsCount;

		public int? edgeDefenseGuardsCount;

		public ThingDef mortarDef;

		public TerrainDef pathwayFloorDef;

		public ThingDef cultivatedPlantDef;

		public int? fillWithThingsPadding;

		public float? settlementPawnGroupPoints;

		public int? settlementPawnGroupSeed;

		public bool? streetHorizontal;

		public bool? edgeThingAvoidOtherEdgeThings;

		public bool? allowPlacementOffEdge;

		public Rot4? thrustAxis;

		public FloatRange? hpPercentRange;

		public void SetCustom<T>(string name, T obj, bool inherit = false)
		{
			if (this.custom == null)
			{
				this.custom = new Dictionary<string, object>();
			}
			else
			{
				this.custom = new Dictionary<string, object>(this.custom);
			}
			if (!this.custom.ContainsKey(name))
			{
				this.custom.Add(name, obj);
			}
			else if (!inherit)
			{
				this.custom[name] = obj;
			}
		}

		public void RemoveCustom(string name)
		{
			if (this.custom != null)
			{
				this.custom = new Dictionary<string, object>(this.custom);
				this.custom.Remove(name);
			}
		}

		public bool TryGetCustom<T>(string name, out T obj)
		{
			object obj2;
			bool result;
			if (this.custom == null || !this.custom.TryGetValue(name, out obj2))
			{
				obj = default(T);
				result = false;
			}
			else
			{
				obj = (T)((object)obj2);
				result = true;
			}
			return result;
		}

		public T GetCustom<T>(string name)
		{
			object obj;
			T result;
			if (this.custom == null || !this.custom.TryGetValue(name, out obj))
			{
				result = default(T);
			}
			else
			{
				result = (T)((object)obj);
			}
			return result;
		}

		public override string ToString()
		{
			object[] array = new object[112];
			array[0] = "rect=";
			array[1] = this.rect;
			array[2] = ", faction=";
			array[3] = ((this.faction == null) ? "null" : this.faction.ToString());
			array[4] = ", custom=";
			array[5] = ((this.custom == null) ? "null" : this.custom.Count.ToString());
			array[6] = ", ancientTempleEntranceHeight=";
			int num = 7;
			int? num2 = this.ancientTempleEntranceHeight;
			array[num] = ((num2 == null) ? "null" : this.ancientTempleEntranceHeight.ToString());
			array[8] = ", pawnGroupMakerParams=";
			array[9] = ((this.pawnGroupMakerParams == null) ? "null" : this.pawnGroupMakerParams.ToString());
			array[10] = ", pawnGroupKindDef=";
			array[11] = ((this.pawnGroupKindDef == null) ? "null" : this.pawnGroupKindDef.ToString());
			array[12] = ", roofDef=";
			array[13] = ((this.roofDef == null) ? "null" : this.roofDef.ToString());
			array[14] = ", noRoof=";
			int num3 = 15;
			bool? flag = this.noRoof;
			array[num3] = ((flag == null) ? "null" : this.noRoof.ToString());
			array[16] = ", addRoomCenterToRootsToUnfog=";
			int num4 = 17;
			bool? flag2 = this.addRoomCenterToRootsToUnfog;
			array[num4] = ((flag2 == null) ? "null" : this.addRoomCenterToRootsToUnfog.ToString());
			array[18] = ", singleThingToSpawn=";
			array[19] = ((this.singleThingToSpawn == null) ? "null" : this.singleThingToSpawn.ToString());
			array[20] = ", singleThingDef=";
			array[21] = ((this.singleThingDef == null) ? "null" : this.singleThingDef.ToString());
			array[22] = ", singleThingStuff=";
			array[23] = ((this.singleThingStuff == null) ? "null" : this.singleThingStuff.ToString());
			array[24] = ", singleThingStackCount=";
			int num5 = 25;
			int? num6 = this.singleThingStackCount;
			array[num5] = ((num6 == null) ? "null" : this.singleThingStackCount.ToString());
			array[26] = ", skipSingleThingIfHasToWipeBuildingOrDoesntFit=";
			int num7 = 27;
			bool? flag3 = this.skipSingleThingIfHasToWipeBuildingOrDoesntFit;
			array[num7] = ((flag3 == null) ? "null" : this.skipSingleThingIfHasToWipeBuildingOrDoesntFit.ToString());
			array[28] = ", singlePawnToSpawn=";
			array[29] = ((this.singlePawnToSpawn == null) ? "null" : this.singlePawnToSpawn.ToString());
			array[30] = ", singlePawnKindDef=";
			array[31] = ((this.singlePawnKindDef == null) ? "null" : this.singlePawnKindDef.ToString());
			array[32] = ", disableSinglePawn=";
			int num8 = 33;
			bool? flag4 = this.disableSinglePawn;
			array[num8] = ((flag4 == null) ? "null" : this.disableSinglePawn.ToString());
			array[34] = ", singlePawnLord=";
			array[35] = ((this.singlePawnLord == null) ? "null" : this.singlePawnLord.ToString());
			array[36] = ", singlePawnSpawnCellExtraPredicate=";
			array[37] = ((this.singlePawnSpawnCellExtraPredicate == null) ? "null" : this.singlePawnSpawnCellExtraPredicate.ToString());
			array[38] = ", singlePawnGenerationRequest=";
			int num9 = 39;
			PawnGenerationRequest? pawnGenerationRequest = this.singlePawnGenerationRequest;
			array[num9] = ((pawnGenerationRequest == null) ? "null" : this.singlePawnGenerationRequest.ToString());
			array[40] = ", postThingSpawn=";
			array[41] = ((this.postThingSpawn == null) ? "null" : this.postThingSpawn.ToString());
			array[42] = ", postThingGenerate=";
			array[43] = ((this.postThingGenerate == null) ? "null" : this.postThingGenerate.ToString());
			array[44] = ", mechanoidsCount=";
			int num10 = 45;
			int? num11 = this.mechanoidsCount;
			array[num10] = ((num11 == null) ? "null" : this.mechanoidsCount.ToString());
			array[46] = ", hivesCount=";
			int num12 = 47;
			int? num13 = this.hivesCount;
			array[num12] = ((num13 == null) ? "null" : this.hivesCount.ToString());
			array[48] = ", disableHives=";
			int num14 = 49;
			bool? flag5 = this.disableHives;
			array[num14] = ((flag5 == null) ? "null" : this.disableHives.ToString());
			array[50] = ", thingRot=";
			int num15 = 51;
			Rot4? rot = this.thingRot;
			array[num15] = ((rot == null) ? "null" : this.thingRot.ToString());
			array[52] = ", wallStuff=";
			array[53] = ((this.wallStuff == null) ? "null" : this.wallStuff.ToString());
			array[54] = ", chanceToSkipWallBlock=";
			int num16 = 55;
			float? num17 = this.chanceToSkipWallBlock;
			array[num16] = ((num17 == null) ? "null" : this.chanceToSkipWallBlock.ToString());
			array[56] = ", floorDef=";
			array[57] = ((this.floorDef == null) ? "null" : this.floorDef.ToString());
			array[58] = ", chanceToSkipFloor=";
			int num18 = 59;
			float? num19 = this.chanceToSkipFloor;
			array[num18] = ((num19 == null) ? "null" : this.chanceToSkipFloor.ToString());
			array[60] = ", filthDef=";
			array[61] = ((this.filthDef == null) ? "null" : this.filthDef.ToString());
			array[62] = ", filthDensity=";
			int num20 = 63;
			FloatRange? floatRange = this.filthDensity;
			array[num20] = ((floatRange == null) ? "null" : this.filthDensity.ToString());
			array[64] = ", clearEdificeOnly=";
			int num21 = 65;
			bool? flag6 = this.clearEdificeOnly;
			array[num21] = ((flag6 == null) ? "null" : this.clearEdificeOnly.ToString());
			array[66] = ", clearFillageOnly=";
			int num22 = 67;
			bool? flag7 = this.clearFillageOnly;
			array[num22] = ((flag7 == null) ? "null" : this.clearFillageOnly.ToString());
			array[68] = ", clearRoof=";
			int num23 = 69;
			bool? flag8 = this.clearRoof;
			array[num23] = ((flag8 == null) ? "null" : this.clearRoof.ToString());
			array[70] = ", ancientCryptosleepCasketGroupID=";
			int num24 = 71;
			int? num25 = this.ancientCryptosleepCasketGroupID;
			array[num24] = ((num25 == null) ? "null" : this.ancientCryptosleepCasketGroupID.ToString());
			array[72] = ", podContentsType=";
			int num26 = 73;
			PodContentsType? podContentsType = this.podContentsType;
			array[num26] = ((podContentsType == null) ? "null" : this.podContentsType.ToString());
			array[74] = ", thingSetMakerDef=";
			array[75] = ((this.thingSetMakerDef == null) ? "null" : this.thingSetMakerDef.ToString());
			array[76] = ", thingSetMakerParams=";
			int num27 = 77;
			ThingSetMakerParams? thingSetMakerParams = this.thingSetMakerParams;
			array[num27] = ((thingSetMakerParams == null) ? "null" : this.thingSetMakerParams.ToString());
			array[78] = ", stockpileConcreteContents=";
			array[79] = ((this.stockpileConcreteContents == null) ? "null" : this.stockpileConcreteContents.Count.ToString());
			array[80] = ", stockpileMarketValue=";
			int num28 = 81;
			float? num29 = this.stockpileMarketValue;
			array[num28] = ((num29 == null) ? "null" : this.stockpileMarketValue.ToString());
			array[82] = ", innerStockpileSize=";
			int num30 = 83;
			int? num31 = this.innerStockpileSize;
			array[num30] = ((num31 == null) ? "null" : this.innerStockpileSize.ToString());
			array[84] = ", edgeDefenseWidth=";
			int num32 = 85;
			int? num33 = this.edgeDefenseWidth;
			array[num32] = ((num33 == null) ? "null" : this.edgeDefenseWidth.ToString());
			array[86] = ", edgeDefenseTurretsCount=";
			int num34 = 87;
			int? num35 = this.edgeDefenseTurretsCount;
			array[num34] = ((num35 == null) ? "null" : this.edgeDefenseTurretsCount.ToString());
			array[88] = ", edgeDefenseMortarsCount=";
			int num36 = 89;
			int? num37 = this.edgeDefenseMortarsCount;
			array[num36] = ((num37 == null) ? "null" : this.edgeDefenseMortarsCount.ToString());
			array[90] = ", edgeDefenseGuardsCount=";
			int num38 = 91;
			int? num39 = this.edgeDefenseGuardsCount;
			array[num38] = ((num39 == null) ? "null" : this.edgeDefenseGuardsCount.ToString());
			array[92] = ", mortarDef=";
			array[93] = ((this.mortarDef == null) ? "null" : this.mortarDef.ToString());
			array[94] = ", pathwayFloorDef=";
			array[95] = ((this.pathwayFloorDef == null) ? "null" : this.pathwayFloorDef.ToString());
			array[96] = ", cultivatedPlantDef=";
			array[97] = ((this.cultivatedPlantDef == null) ? "null" : this.cultivatedPlantDef.ToString());
			array[98] = ", fillWithThingsPadding=";
			int num40 = 99;
			int? num41 = this.fillWithThingsPadding;
			array[num40] = ((num41 == null) ? "null" : this.fillWithThingsPadding.ToString());
			array[100] = ", settlementPawnGroupPoints=";
			int num42 = 101;
			float? num43 = this.settlementPawnGroupPoints;
			array[num42] = ((num43 == null) ? "null" : this.settlementPawnGroupPoints.ToString());
			array[102] = ", settlementPawnGroupSeed=";
			int num44 = 103;
			int? num45 = this.settlementPawnGroupSeed;
			array[num44] = ((num45 == null) ? "null" : this.settlementPawnGroupSeed.ToString());
			array[104] = ", streetHorizontal=";
			int num46 = 105;
			bool? flag9 = this.streetHorizontal;
			array[num46] = ((flag9 == null) ? "null" : this.streetHorizontal.ToString());
			array[106] = ", edgeThingAvoidOtherEdgeThings=";
			int num47 = 107;
			bool? flag10 = this.edgeThingAvoidOtherEdgeThings;
			array[num47] = ((flag10 == null) ? "null" : this.edgeThingAvoidOtherEdgeThings.ToString());
			array[108] = ", allowPlacementOffEdge=";
			int num48 = 109;
			bool? flag11 = this.allowPlacementOffEdge;
			array[num48] = ((flag11 == null) ? "null" : this.allowPlacementOffEdge.ToString());
			array[110] = ", thrustAxis=";
			int num49 = 111;
			Rot4? rot2 = this.thrustAxis;
			array[num49] = ((rot2 == null) ? "null" : this.thrustAxis.ToString());
			return string.Concat(array);
		}
	}
}
