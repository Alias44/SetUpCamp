using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SetUpCamp;

/// <summary>
/// Custom comp for persisting camp maps when no colonists are present
/// </summary>
[StaticConstructorOnStartup]
public class PermaCampComp : WorldObjectComp
{
	public bool persistent = false;

	// todo: create custom claim variants
	private static readonly Texture2D offTex = ContentFinder<Texture2D>.Get("UI/Designators/HomeAreaOff");
	private static readonly Texture2D onTex = ContentFinder<Texture2D>.Get("UI/Designators/HomeAreaOn");

	/// <summary>
	/// Not strictly a necessary feature, but checking the settings value at the time of map creation allows the user to change the setting whenever
	/// </summary>
	public override void PostMapGenerate()
	{
		persistent = SetUpCampSettings.persistCampsDefault;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		// Hide the gizmo if the setting is disabled (this prevents needing weirdness to dynamically assign/ remove the comps at runtime)
		// and the camp isn't already persistent (this ensures that persistent camps can always be unmarked, even if the setting has been changed)
		if (!SetUpCampSettings.persistCamps && !persistent)
		{
			yield break;
		}

		Command_Action action = new Command_Action();;
		action.Order = 3000f;

		if (persistent)
		{
			action.defaultLabel = "UnmarkPersistent".Translate();
			action.defaultDesc = "UnmarkPersistentDesc".Translate();
			action.icon = offTex;
			action.action = delegate
			{
				
				Camp c = this.parent as Camp;
				if (!c.Map.mapPawns.AnyPawnBlockingMapRemoval && SetUpCampSettings.confirmEmptyCamps)
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmUnmarkEmptyCamp".Translate(), () => persistent = false));
				}
				else
				{
					persistent = false;
				}
			};
		}
		else
		{
			action.defaultLabel = "MarkPersistent".Translate();
			action.defaultDesc = "PersistentDesc".Translate();
			action.icon = onTex;
			action.action = delegate
			{
				persistent = true;
			};
		}


		yield return action;
	}

	public override string CompInspectStringExtra()
	{
		if (!SetUpCampSettings.persistCamps)
		{
			return "";
		}

		return (persistent ? "Persistent" : "").Translate();
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref persistent, "persistent");
	}
}

public class WorldObjectCompProperties_PermaCamp : WorldObjectCompProperties
{
	public WorldObjectCompProperties_PermaCamp() {
		this.compClass = typeof(PermaCampComp);
	}

	public override IEnumerable<string> ConfigErrors(WorldObjectDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (!typeof(Camp).IsAssignableFrom(parentDef.worldObjectClass))
		{
			yield return parentDef.defName + " has WorldObjectCompProperties_PermaCamp but it's not type Camp.";
		}
	}
}