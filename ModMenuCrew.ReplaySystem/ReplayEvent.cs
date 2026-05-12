using UnityEngine;

namespace ModMenuCrew.ReplaySystem;

public class ReplayEvent
{
	public float Time;

	public ReplayEventType Type;

	public byte PlayerId;

	public byte TargetId;

	public Vector2 Position;

	public string Description;

	public string GetIcon()
	{
		return Type switch
		{
			ReplayEventType.Kill => "\ud83d\udc80", 
			ReplayEventType.Meeting => "\ud83d\udce2", 
			ReplayEventType.Vote => "✋", 
			ReplayEventType.Exiled => "\ud83d\ude80", 
			ReplayEventType.Sabotage => "⚠\ufe0f", 
			ReplayEventType.SabotageFixed => "\ud83d\udd27", 
			ReplayEventType.TaskComplete => "✅", 
			ReplayEventType.Vent => "\ud83d\udd73\ufe0f", 
			ReplayEventType.Shapeshift => "\ud83c\udfad", 
			ReplayEventType.Disconnect => "\ud83d\udcf4", 
			ReplayEventType.GameStart => "\ud83c\udfae", 
			ReplayEventType.GameEnd => "\ud83c\udfc1", 
			ReplayEventType.Report => "\ud83d\udea8", 
			ReplayEventType.Chat => "\ud83d\udcac", 
			ReplayEventType.DoorClose => "\ud83d\udeaa", 
			ReplayEventType.DoorOpen => "\ud83d\udeaa", 
			ReplayEventType.LightsSabotage => "\ud83d\udca1", 
			ReplayEventType.ReactorSabotage => "☢\ufe0f", 
			ReplayEventType.O2Sabotage => "\ud83e\udec1", 
			ReplayEventType.CommsSabotage => "\ud83d\udce1", 
			ReplayEventType.EmergencyButton => "\ud83d\udd34", 
			_ => "❓", 
		};
	}

	public Color GetColor()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		return (Color)(Type switch
		{
			ReplayEventType.Kill => new Color(1f, 0.2f, 0.2f), 
			ReplayEventType.Meeting => new Color(1f, 0.9f, 0.2f), 
			ReplayEventType.Vote => new Color(0.4f, 0.8f, 1f), 
			ReplayEventType.Exiled => new Color(1f, 0.5f, 0f), 
			ReplayEventType.Sabotage => new Color(1f, 0.3f, 0.3f), 
			ReplayEventType.SabotageFixed => new Color(0.3f, 1f, 0.3f), 
			ReplayEventType.TaskComplete => new Color(0.3f, 1f, 0.5f), 
			ReplayEventType.Vent => new Color(0.5f, 0.5f, 0.5f), 
			ReplayEventType.Shapeshift => new Color(0.8f, 0.4f, 1f), 
			ReplayEventType.Report => new Color(0.2f, 0.4f, 1f), 
			ReplayEventType.Chat => new Color(0.9f, 0.9f, 0.9f), 
			ReplayEventType.DoorClose => new Color(0.6f, 0.4f, 0.2f), 
			ReplayEventType.DoorOpen => new Color(0.4f, 0.7f, 0.3f), 
			ReplayEventType.LightsSabotage => new Color(1f, 0.85f, 0.1f), 
			ReplayEventType.ReactorSabotage => new Color(1f, 0.4f, 0f), 
			ReplayEventType.O2Sabotage => new Color(0.3f, 0.7f, 1f), 
			ReplayEventType.CommsSabotage => new Color(0.6f, 0.6f, 0.8f), 
			ReplayEventType.EmergencyButton => new Color(1f, 0.3f, 0.3f), 
			_ => Color.white, 
		});
	}
}
