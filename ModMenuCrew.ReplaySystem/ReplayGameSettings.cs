using System.IO;

namespace ModMenuCrew.ReplaySystem;

public class ReplayGameSettings
{
	public float PlayerSpeed = 1f;

	public float CrewmateVision = 1f;

	public float ImpostorVision = 1.5f;

	public float KillCooldown = 25f;

	public float KillDistance = 1f;

	public int NumCommonTasks = 1;

	public int NumLongTasks = 1;

	public int NumShortTasks = 2;

	public float EmergencyButtonCooldown = 15f;

	public int MaxEmergencyCalls = 1;

	public bool ConfirmEjects = true;

	public bool AnonymousVotes;

	public bool VisualTasks = true;

	public string GameMode = "Classic";

	public void Write(BinaryWriter w)
	{
		w.Write(PlayerSpeed);
		w.Write(CrewmateVision);
		w.Write(ImpostorVision);
		w.Write(KillCooldown);
		w.Write(KillDistance);
		w.Write(NumCommonTasks);
		w.Write(NumLongTasks);
		w.Write(NumShortTasks);
		w.Write(EmergencyButtonCooldown);
		w.Write(MaxEmergencyCalls);
		w.Write(ConfirmEjects);
		w.Write(AnonymousVotes);
		w.Write(VisualTasks);
		w.Write(GameMode ?? "Classic");
	}

	public static ReplayGameSettings Read(BinaryReader r)
	{
		return new ReplayGameSettings
		{
			PlayerSpeed = r.ReadSingle(),
			CrewmateVision = r.ReadSingle(),
			ImpostorVision = r.ReadSingle(),
			KillCooldown = r.ReadSingle(),
			KillDistance = r.ReadSingle(),
			NumCommonTasks = r.ReadInt32(),
			NumLongTasks = r.ReadInt32(),
			NumShortTasks = r.ReadInt32(),
			EmergencyButtonCooldown = r.ReadSingle(),
			MaxEmergencyCalls = r.ReadInt32(),
			ConfirmEjects = r.ReadBoolean(),
			AnonymousVotes = r.ReadBoolean(),
			VisualTasks = r.ReadBoolean(),
			GameMode = r.ReadString()
		};
	}
}
