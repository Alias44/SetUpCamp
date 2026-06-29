using UnityEngine;
using Verse;

namespace SetUpCamp;

/// <summary>
/// Listing_Standard subclass to allow for more customization in settings menu formatting.
/// </summary>
public class CustomListing : Listing_Standard
{
	/// <summary>
	/// Numeric entry with tooltip and modified text/ input split.
	/// </summary>
	/// <seealso cref="Listing_Standard.TextFieldNumericLabeled{T}(string, ref T, ref string, float, float)"/>
	public void TextFieldNumericLabeled<T>(string label, ref T val, ref string buffer, string tooltip = null, float min = 0f, float max = 1E+09f, float split = 0.5f) where T : struct
	{
		//The game always assumes listings will be a single line. If needed `Text.CalcHeight(label, ColumnWidth)` will accomidate multiple lines
		Rect rect = GetRect(Text.LineHeight);
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			if (!tooltip.NullOrEmpty())
			{
				if (Mouse.IsOver(rect))
				{
					Widgets.DrawHighlight(rect);
				}
				TooltipHandler.TipRegion(rect, tooltip);
			}

			TextFieldNumericLabeled(rect, label, ref val, ref buffer, min, max, split);
		}
		Gap(verticalSpacing);
	}

	/// <summary>
	/// Modified Widget to allow altering the allocation of space between the text and input.
	/// </summary>
	/// <remarks>Widgets is static and can't be extended, so this lives with the other custom UI code</remarks>
	/// <seealso cref="Widgets.TextFieldNumericLabeled{T}(Rect, string, ref T, ref string, float, float)"/>
	public void TextFieldNumericLabeled<T>(Rect rect, string label, ref T val, ref string buffer, float min = 0f, float max = 1E+09f, float split = 0.5f) where T : struct
	{
		Rect textArea = rect.LeftPart(split).Rounded();
		Rect inputArea = rect.RightPart(1 - split).Rounded();
		TextAnchor anchor = Text.Anchor;
		Text.Anchor = TextAnchor.MiddleRight;
		Widgets.Label(textArea, label);
		Text.Anchor = anchor;
		Widgets.TextFieldNumeric(inputArea, ref val, ref buffer, min, max);
	}

}