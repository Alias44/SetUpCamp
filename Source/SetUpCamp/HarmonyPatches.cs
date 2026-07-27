using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace SetUpCamp;

public class HarmonyPatches : Mod
{
	public HarmonyPatches(ModContentPack content) : base(content)
	{
		var harmony = new Harmony("SetUpCamp.main");

		harmony.Patch(AccessTools.Method(typeof(Camp), "Notify_MyMapRemoved"), postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CampMapRemoved)));
		harmony.Patch(AccessTools.Method(typeof(Camp), "ShouldRemoveMapNow"), postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CampShouldRemoveMapNow)));

		harmony.Patch(AccessTools.Method(typeof(SettleInEmptyTileUtility), "SetupCamp"), postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(SetupCamp)));

		harmony.Patch(AccessTools.Method(typeof(GenStep_ScatterLumpsMineable), "Generate"), prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(GenerateResources)));

		// Add a custom back compatibility to the conversion chain
		List<BackCompatibilityConverter> compatibilityConverters =
			AccessTools.StaticFieldRefAccess<List<BackCompatibilityConverter>>(typeof(BackCompatibility),
				"conversionChain");

		compatibilityConverters.Add(new BackCompatibilityConverter_Camp());
	}

	/// <summary>Preempt camp ruin generation and substitute hardcoded timer with modified value</summary>
	/// <remarks>This avoids having to create a transpiler for the method</remarks>
	/// <seealso cref="Camp.Notify_MyMapRemoved(Map)"/>
	[HarmonyPostfix]
	public static void CampMapRemoved(Camp __instance, Map map)
	{
		var abandonedCamp = Find.WorldObjects.WorldObjectOfDefAt(WorldObjectDefOf.AbandonedCamp, __instance.Tile);

		abandonedCamp?.GetComponent<TimeoutComp>().StartTimeout(SetUpCampSettings.RuinTicks);
	}

	public static void CampShouldRemoveMapNow(Camp __instance, ref bool alsoRemoveWorldObject, ref bool __result)
	{
		if (__result && SetUpCampSettings.persistCamps)
		{
			__result = !__instance.GetComponent<PermaCampComp>().persistent;
			alsoRemoveWorldObject = __result;
		}
	}

	/// <summary>The innermost delegate of the SetupCamp method, modified to add custom map size and raid timeouts</summary>
	public static void GenerateCamp(Caravan caravan)
	{
		IntVec3 mapSize = SetUpCampSettings.campSize ?? WorldObjectDefOf.Camp.overrideMapSize ?? Find.World.info.initialMapSize;
		Map map = GetOrGenerateMapUtility.GetOrGenerateMap(caravan.Tile, mapSize, WorldObjectDefOf.Camp);
		map.Parent.SetFaction(caravan.Faction);
		Pawn pawn = caravan.PawnsListForReading[0];
		CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Center, CaravanDropInventoryMode.DoNotDrop, draftColonists: false, delegate (IntVec3 x)
		{
			if (x.GetRoom(map).CellCount < 600)
			{
				return false;
			}
			return !x.GetTerrain(map).IsWater;
		});

		if (SetUpCampSettings.RaidTicks > 0)
		{
			map.Parent.GetComponent<TimedDetectionRaids>()?.StartDetectionCountdown(SetUpCampSettings.RaidTicks, SetUpCampSettings.RaidWarnTicks);
		}

		CameraJumper.TryJump(pawn);

	}

	/// <summary>Modify the code used for camp generation to add custom map size and raid timeouts</summary>
	/// <remarks>Replacing the vanilla code isn't ideal, but this is the least hacky way I could think to go about (without having to traverse the delegate chain and create a transpiler for the method).</remarks>
	[HarmonyPostfix]
	public static void SetupCamp(ref Command __result, Caravan caravan)
	{
		((Command_Action) __result).action = delegate
		{
			LongEventHandler.QueueLongEvent(() => GenerateCamp(caravan), "GeneratingMap", doAsynchronously: true, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
		};
	}

	/// <summary>Modify map generation step to allows camp maps to have resources (makes RocksFromGrid_NoMinerals dynamically behave like RocksFromGrid)</summary>
	/// <remarks>Technically, the responsible logic is in GenStep_RocksFromGrid.Generate(), but tweaking that would require a transpiler to target and modify the branch chanin.</remarks>
	[HarmonyPrefix]
	public static void GenerateResources(GenStep_ScatterLumpsMineable __instance, ref Map map)
	{
		// maxValue = 0 comes from the maxMineableValue in RocksFromGrid_NoMinerals
		if (!SetUpCampSettings.campResources && __instance.maxValue == 0 && map.generatorDef.Equals(MapGeneratorDefOf.Encounter) && map.Parent.def.Equals(WorldObjectDefOf.Camp))
		{
			__instance.maxValue = float.MaxValue;
		}
	}
}
