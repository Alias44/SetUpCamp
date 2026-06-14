using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace SetUpCamp;

public class HarmonyPatches : Mod
{
	public HarmonyPatches(ModContentPack content) : base(content)
	{
		var harmony = new Harmony("SetUpCamp.main");

		harmony.Patch(AccessTools.Method(typeof(Camp), "Notify_MyMapRemoved"), postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CampMapRemoved)));
		harmony.Patch(AccessTools.Method(typeof(SettleInEmptyTileUtility), "SetupCamp"), postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(SetupCamp)));
	}

	/// <summary>Prempt camp ruin generation and subsitute hardcoded timer with modified value</summary>
	/// <remarks>This avoids having to create a transpiler for the method</remarks>
	/// <seealso cref="Camp.Notify_MyMapRemoved(Map)"/>
	[HarmonyPostfix]
	public static void CampMapRemoved(Camp __instance, Map map)
	{
		var abandonedCamp = Find.WorldObjects.WorldObjectOfDefAt(WorldObjectDefOf.AbandonedCamp, __instance.Tile);

		abandonedCamp?.GetComponent<TimeoutComp>().StartTimeout(SetUpCampSettings.RuinTicks);
	}

	/// <summary> The innermost delagate of the SetupCamp method, modified to add custom map size and raid timeouts</summary>
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
	/// <remarks>Replacing the vanilla code isn't ideal, but this is the least hacky way I could think to go about (without having to traverse the delegate chanin and create a transpiler for the method).</remarks>
	[HarmonyPostfix]
	public static void SetupCamp(ref Command __result, Caravan caravan)
	{
		((Command_Action) __result).action = delegate
		{
			LongEventHandler.QueueLongEvent(() => GenerateCamp(caravan), "GeneratingMap", doAsynchronously: true, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
		};
	}
}
