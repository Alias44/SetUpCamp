using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Xml;
using Verse;

namespace SetUpCamp;

public class BackCompatibilityConverter_Camp : BackCompatibilityConverter
{
	Dictionary<string, string> renames = new Dictionary<string, string>()
		{
			{ "CaravanCamp", "Camp" },
			{ "Syr_SetUpCamp", "Encounter" },
			{ "Syr_SetUpCampNR", "Encounter" },
		};

	public override bool AppliesToVersion(int majorVer, int minorVer) => majorVer == 1 && minorVer <= 5;

	public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
	{
		if (GenDefDatabase.GetDefSilentFail(defType, defName, false) == null)
		{
			if (defType == typeof(WorldObjectDef) || defType == typeof(MapGeneratorDef))
			{
				return renames.TryGetValue(defName);
			}
		}
		return null;
	}

	public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
	{
		if (providedClassName.Equals("Syrchalis_SetUpCamp.CaravanCamp"))
		{
			return typeof(Camp);
		}

		return null;
	}

	public override void PostExposeData(object obj)
	{
		return;
	}
}
