using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace SetUpCamp;

public class SetUpCampMod : Mod
{
	public SetUpCampSettings settings;

	public static readonly int[] campMapSizes = [100, 150, .. Dialog_AdvancedGameConfig.MapSizes];
	public static readonly int[] testMapSizes = AccessTools.StaticFieldRefAccess<int[]>(typeof(Dialog_AdvancedGameConfig), "TestMapSizes");

	public SetUpCampMod(ModContentPack content) : base(content)
	{
		settings = GetSettings<SetUpCampSettings>();
	}

	public override string SettingsCategory() => "SetUpCampSettings".Translate();

	string ruinBuff;
	string raidBuff;
	public override void DoSettingsWindowContents(Rect inRect)
	{
		// Initalize menu values (this is done in method level because Mod initalization occurs before the settings ExposeData is called)
		ruinBuff = SetUpCampSettings.ruinDuration.ToString();
		raidBuff = SetUpCampSettings.raidTimer.ToString();

		IEnumerable<int> mapSizes = campMapSizes.AsEnumerable();

		if (Prefs.TestMapSizes)
		{
			mapSizes = mapSizes.Concat(testMapSizes);
		}

		CustomListing listing = new();


		listing.Begin(inRect);

		listing.ColumnWidth = (inRect.width - Listing.ColumnSpacing) / 2;

		Color defaultColor = GUI.color;
		GUI.color = Color.yellow;
		listing.Label("SetUpCampNote".Translate());
		GUI.color = defaultColor;

		if (listing.ButtonText("Reset".Translate()))
		{
			SetUpCampSettings.Reset();
		}

		listing.GapLine();
		listing.Gap();

		listing.CheckboxLabeled("CampResources".Translate(), ref SetUpCampSettings.campResources, "CampResourcesTooltip".Translate());
		listing.Gap();

		listing.TextFieldNumericLabeled("RaidTimer".Translate(), ref SetUpCampSettings.raidTimer, ref raidBuff, "RaidTimerTooltip".Translate(), 0, split: 0.75f);
		listing.Gap();

		listing.TextFieldNumericLabeled("AbandonedCampDuration".Translate(), ref SetUpCampSettings.ruinDuration, ref ruinBuff, "AbandonedCampDurationTooltip".Translate(), 0, split: 0.75f);

		listing.NewColumn();
		listing.CheckboxLabeled("OverideCampSize".Translate(), ref SetUpCampSettings.overideCampSize);

		if (SetUpCampSettings.overideCampSize)
		{
			// This is hevaily derived from the Dialog_AdvancedGameConfig.DoWindowContents()
			Text.Font = GameFont.Medium;
			listing.Label("MapSize".Translate());

			Text.Font = GameFont.Small;

			foreach (var item in mapSizes)
			{
				// interleaves size headings into the radio list
				switch (item)
				{
					case 100: // this should match campMapSizes[0]
						listing.Label("MapSizeMini".Translate());
						break;
					case 200:
						listing.Gap(10f);
						listing.Label("MapSizeSmall".Translate());
						break;
					case 250:
						listing.Gap(10f);
						listing.Label("MapSizeMedium".Translate());
						break;
					case 300:
						listing.Gap(10f);
						listing.Label("MapSizeLarge".Translate());
						break;
					case 350:
						listing.Gap(10f);
						listing.Label("MapSizeExtreme".Translate());
						break;
				}

				if (listing.RadioButton("MapSizeDesc".Translate(item, item * item), SetUpCampSettings.campSize?.x == item))
				{
					SetUpCampSettings.campSize = new IntVec3(item, 1, item);
				}
			}
		}
		else
		{
			SetUpCampSettings.campSize = null;
		}

		listing.End();
	}

}

public class SetUpCampSettings : ModSettings
{
	///<summary>Hardcoded in <see cref="RimWorld.Planet.Camp.Notify_MyMapRemoved(Map)"/></summary>
	public const int baseRuinTicks = 1800000;
	public static readonly float baseRuinDays = GenDate.TicksToDays(baseRuinTicks);

	///<summary>Hardcoded in <see cref="RimWorld.Planet.SettleInEmptyTileUtility.SetupCamp(RimWorld.Planet.Caravan)"/></summary>
	public const int baseRaidTicks = 240000;
	public static readonly float baseRaidDays = GenDate.TicksToDays(baseRaidTicks);

	///<summary>Hardcoded in <see cref="RimWorld.Planet.SettleInEmptyTileUtility.SetupCamp(RimWorld.Planet.Caravan)"/></summary>
	public const int baseRaidWarnTicks = 60000;

	public static bool overideCampSize = false;
	public static IntVec3? campSize;
	public static float ruinDuration = baseRuinDays;
	public static float raidTimer = baseRaidDays;

	public static bool campResources = false;

	public static int RuinTicks => GenDate.DaysToTicks(ruinDuration);

	public static int RaidTicks => GenDate.DaysToTicks(raidTimer);
	public static int RaidWarnTicks => GenDate.DaysToTicks(raidTimer * (baseRaidWarnTicks/baseRaidTicks));

	public override void ExposeData()
	{
		Scribe_Values.Look(ref campSize, "customCampSize");
		Scribe_Values.Look(ref ruinDuration, "ruinDuration");
		Scribe_Values.Look(ref raidTimer, "raidTimer");

		if (Scribe.mode == LoadSaveMode.LoadingVars && campSize?.x != 0)
		{
			overideCampSize = true;
		}
	}

	public static void Reset()
	{
		overideCampSize = false;
		campSize = null;
		ruinDuration = baseRuinDays;
		raidTimer = baseRaidDays;
		campResources = false;
	}
}
