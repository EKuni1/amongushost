namespace ModMenuCrew.Web;

public struct PlayerData
{
	public byte Id { get; set; }

	public string Name { get; set; }

	public float X { get; set; }

	public float Y { get; set; }

	public int ColorId { get; set; }

	public bool IsImpostor { get; set; }

	public bool IsDead { get; set; }

	public bool InVent { get; set; }
}
