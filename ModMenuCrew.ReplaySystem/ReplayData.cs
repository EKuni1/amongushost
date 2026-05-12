using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ModMenuCrew.ReplaySystem;

public class ReplayData
{
	public const string MagicHeader = "AMR";

	public const int CurrentVersion = 5;

	public string GameVersion;

	public int MapId;

	public string MapName;

	public DateTime RecordedAt;

	public float TotalDuration;

	public ReplayGameSettings Settings = new ReplayGameSettings();

	public List<ReplayPlayerInfo> Players = new List<ReplayPlayerInfo>();

	public List<ReplayFrame> Frames = new List<ReplayFrame>();

	public List<ReplayEvent> Events = new List<ReplayEvent>();

	public void Save(string path)
	{
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		using FileStream output = new FileStream(path, FileMode.Create);
		using BinaryWriter binaryWriter = new BinaryWriter(output);
		binaryWriter.Write("AMR".ToCharArray());
		binaryWriter.Write(5);
		binaryWriter.Write(GameVersion ?? "Unknown");
		binaryWriter.Write(MapId);
		binaryWriter.Write(MapName ?? "Unknown");
		binaryWriter.Write(RecordedAt.ToBinary());
		binaryWriter.Write(TotalDuration);
		Settings.Write(binaryWriter);
		binaryWriter.Write(Players.Count);
		foreach (ReplayPlayerInfo player in Players)
		{
			binaryWriter.Write(player.PlayerId);
			binaryWriter.Write(player.Name ?? "Unknown");
			binaryWriter.Write(player.ColorId);
			binaryWriter.Write(player.HatId ?? "");
			binaryWriter.Write(player.SkinId ?? "");
			binaryWriter.Write(player.PetId ?? "");
			binaryWriter.Write(player.IsImpostor);
			binaryWriter.Write(player.RealColor.r);
			binaryWriter.Write(player.RealColor.g);
			binaryWriter.Write(player.RealColor.b);
			binaryWriter.Write(player.RealColor.a);
			binaryWriter.Write(player.VisorId ?? "");
			binaryWriter.Write(player.NamePlateId ?? "");
			binaryWriter.Write(player.RoleName ?? "");
			binaryWriter.Write(player.IsImpostorTeam);
		}
		binaryWriter.Write(Frames.Count);
		foreach (ReplayFrame frame in Frames)
		{
			binaryWriter.Write(frame.Time);
			binaryWriter.Write((byte)frame.States.Count);
			foreach (PlayerState state in frame.States)
			{
				binaryWriter.Write(state.PlayerId);
				binaryWriter.Write(state.Position.x);
				binaryWriter.Write(state.Position.y);
				binaryWriter.Write(state.FaceRight);
				binaryWriter.Write(state.IsDead);
				binaryWriter.Write(state.IsInVent);
				binaryWriter.Write((byte)state.AnimState);
			}
		}
		binaryWriter.Write(Events.Count);
		foreach (ReplayEvent @event in Events)
		{
			binaryWriter.Write(@event.Time);
			binaryWriter.Write((byte)@event.Type);
			binaryWriter.Write(@event.PlayerId);
			binaryWriter.Write(@event.TargetId);
			binaryWriter.Write(@event.Position.x);
			binaryWriter.Write(@event.Position.y);
			binaryWriter.Write(@event.Description ?? "");
		}
	}

	public static ReplayData Load(string path)
	{
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		ReplayData replayData = new ReplayData();
		if (!File.Exists(path))
		{
			return null;
		}
		using (FileStream input = new FileStream(path, FileMode.Open))
		{
			using BinaryReader binaryReader = new BinaryReader(input);
			if (new string(binaryReader.ReadChars(3)) != "AMR")
			{
				throw new Exception("Invalid replay file format");
			}
			int num = binaryReader.ReadInt32();
			switch (num)
			{
			default:
				throw new Exception($"Invalid replay version: {num}");
			case 6:
			case 7:
			case 8:
			case 9:
			case 10:
			case 11:
			case 12:
			case 13:
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
			case 19:
			case 20:
			case 21:
			case 22:
			case 23:
			case 24:
			case 25:
			case 26:
			case 27:
			case 28:
			case 29:
			case 30:
			case 31:
			case 32:
			case 33:
			case 34:
			case 35:
			case 36:
			case 37:
			case 38:
			case 39:
			case 40:
			case 41:
			case 42:
			case 43:
			case 44:
			case 45:
			case 46:
			case 47:
			case 48:
			case 49:
			case 50:
			case 51:
			case 52:
			case 53:
			case 54:
			case 55:
			case 56:
			case 57:
			case 58:
			case 59:
			case 60:
			case 61:
			case 62:
			case 63:
			case 64:
			case 65:
			case 66:
			case 67:
			case 68:
			case 69:
			case 70:
			case 71:
			case 72:
			case 73:
			case 74:
			case 75:
			case 76:
			case 77:
			case 78:
			case 79:
			case 80:
			case 81:
			case 82:
			case 83:
			case 84:
			case 85:
			case 86:
			case 87:
			case 88:
			case 89:
			case 90:
			case 91:
			case 92:
			case 93:
			case 94:
			case 95:
			case 96:
			case 97:
			case 98:
			case 99:
			case 100:
				throw new Exception($"Replay version {num} is too new (max {5})");
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			{
				replayData.GameVersion = binaryReader.ReadString();
				replayData.MapId = binaryReader.ReadInt32();
				if (num >= 2)
				{
					replayData.MapName = binaryReader.ReadString();
					replayData.RecordedAt = DateTime.FromBinary(binaryReader.ReadInt64());
					replayData.TotalDuration = binaryReader.ReadSingle();
				}
				if (num >= 5)
				{
					replayData.Settings = ReplayGameSettings.Read(binaryReader);
				}
				int num2 = binaryReader.ReadInt32();
				if (num2 < 0 || num2 > 50)
				{
					throw new Exception($"Invalid player count: {num2}");
				}
				for (int i = 0; i < num2; i++)
				{
					ReplayPlayerInfo replayPlayerInfo = new ReplayPlayerInfo
					{
						PlayerId = binaryReader.ReadByte(),
						Name = binaryReader.ReadString(),
						ColorId = binaryReader.ReadInt32(),
						HatId = binaryReader.ReadString(),
						SkinId = binaryReader.ReadString(),
						PetId = binaryReader.ReadString(),
						IsImpostor = binaryReader.ReadBoolean()
					};
					if (num >= 3)
					{
						byte b = binaryReader.ReadByte();
						byte b2 = binaryReader.ReadByte();
						byte b3 = binaryReader.ReadByte();
						byte b4 = binaryReader.ReadByte();
						replayPlayerInfo.RealColor = new Color32(b, b2, b3, b4);
					}
					else
					{
						replayPlayerInfo.RealColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
					}
					if (num >= 4)
					{
						replayPlayerInfo.VisorId = binaryReader.ReadString();
						replayPlayerInfo.NamePlateId = binaryReader.ReadString();
					}
					if (num >= 5)
					{
						replayPlayerInfo.RoleName = binaryReader.ReadString();
						replayPlayerInfo.IsImpostorTeam = binaryReader.ReadBoolean();
					}
					else
					{
						replayPlayerInfo.RoleName = (replayPlayerInfo.IsImpostor ? "Impostor" : "Crewmate");
						replayPlayerInfo.IsImpostorTeam = replayPlayerInfo.IsImpostor;
					}
					replayData.Players.Add(replayPlayerInfo);
				}
				int num3 = binaryReader.ReadInt32();
				if (num3 < 0 || num3 > 2000000)
				{
					throw new Exception($"Invalid frame count: {num3}");
				}
				for (int j = 0; j < num3; j++)
				{
					ReplayFrame replayFrame = new ReplayFrame();
					replayFrame.Time = binaryReader.ReadSingle();
					int num4 = binaryReader.ReadByte();
					for (int k = 0; k < num4; k++)
					{
						PlayerState playerState = default(PlayerState);
						playerState.PlayerId = binaryReader.ReadByte();
						playerState.Position = new Vector2(binaryReader.ReadSingle(), binaryReader.ReadSingle());
						PlayerState item = playerState;
						if (num < 3)
						{
							binaryReader.ReadSingle();
							binaryReader.ReadSingle();
						}
						item.FaceRight = binaryReader.ReadBoolean();
						item.IsDead = binaryReader.ReadBoolean();
						if (num >= 2)
						{
							item.IsInVent = binaryReader.ReadBoolean();
						}
						if (num >= 3)
						{
							item.AnimState = (AnimState)binaryReader.ReadByte();
						}
						replayFrame.States.Add(item);
					}
					replayData.Frames.Add(replayFrame);
				}
				if (num >= 2)
				{
					int num5 = binaryReader.ReadInt32();
					if (num5 < 0)
					{
						num5 = 0;
					}
					if (num5 > 500000)
					{
						num5 = 500000;
					}
					for (int l = 0; l < num5; l++)
					{
						replayData.Events.Add(new ReplayEvent
						{
							Time = binaryReader.ReadSingle(),
							Type = (ReplayEventType)binaryReader.ReadByte(),
							PlayerId = binaryReader.ReadByte(),
							TargetId = binaryReader.ReadByte(),
							Position = new Vector2(binaryReader.ReadSingle(), binaryReader.ReadSingle()),
							Description = binaryReader.ReadString()
						});
					}
				}
				break;
			}
			}
		}
		return replayData;
	}
}
