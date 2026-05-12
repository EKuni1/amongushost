using System.Collections.Generic;

namespace ModMenuCrew;

internal sealed class TabDefinition
{
	internal string Id;

	internal string Name;

	internal string Icon;

	internal string Context;

	internal bool Enabled;

	internal List<SectionDefinition> Sections = new List<SectionDefinition>();

	internal List<TeleportLocation> Locations = new List<TeleportLocation>();
}
