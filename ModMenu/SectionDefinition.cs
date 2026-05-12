using System.Collections.Generic;

namespace ModMenuCrew;

internal sealed class SectionDefinition
{
	internal string Id;

	internal string Name;

	internal bool Visible = true;

	internal string VisibleWhen;

	internal List<ButtonDefinition> Buttons = new List<ButtonDefinition>();

	internal List<SliderDefinition> Sliders = new List<SliderDefinition>();
}
