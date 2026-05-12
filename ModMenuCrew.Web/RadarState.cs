namespace ModMenuCrew.Web;

public struct RadarState
{
	public string MapName { get; set; }

	public PlayerData[] Players { get; set; }

	public byte LocalPlayerId { get; set; }

	public long Timestamp { get; set; }
}
