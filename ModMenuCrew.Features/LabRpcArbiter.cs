using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using Il2CppSystem;
using InnerNet;
using UnityEngine;

namespace ModMenuCrew.Features;

public static class LabRpcArbiter
{
	public enum Consumer
	{
		GhostTwins,
		ShadowClones,
		MapOps,
		Cleanup
	}

	public enum Priority
	{
		Critical,
		High,
		Normal,
		Low
	}

	public sealed class LabOp
	{
		public Consumer Consumer;

		public Priority Priority;

		public int TargetClientId;

		public SendOption SendOption;

		public Action<MessageWriter> WriteSubMessage;

		public int EstimatedBytes;
	}

	[HarmonyPatch(typeof(InnerNetClient), "HandleDisconnect")]
	public static class HandleDisconnectCapturePatch
	{
		public static void Prefix(DisconnectReasons reason, string stringReason)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			LastKickReason = reason;
			LastKickInfo = stringReason ?? "";
			LastKickTime = Time.time;
			Debug.Log(Object.op_Implicit($"[KICK] reason={reason} info={(string.IsNullOrEmpty(stringReason) ? "<null>" : stringReason)} t={Time.time:F2}"));
			try
			{
				FlushAll();
				_consecutiveSendFailures = 0;
			}
			catch
			{
			}
		}
	}

	private const int BATCH_MTU_RELIABLE = 480;

	private const int BATCH_MTU_UNRELIABLE = 350;

	private const int MAX_QUEUE_PER_CONSUMER = 400;

	private static readonly int ConsumerCount;

	private static readonly int[] _weights;

	private static readonly bool[] _active;

	private static readonly LinkedList<LabOp>[] _queues;

	private static readonly Queue<float> _sendTimestamps;

	private static int _criticalRobinIdx;

	private static int _consecutiveSendFailures;

	private const int MAX_CONSECUTIVE_FAILURES = 3;

	public static DisconnectReasons LastKickReason;

	public static string LastKickInfo;

	public static float LastKickTime;

	private static int HARD_CAP_PER_SEC
	{
		get
		{
			if (ServerData.Config.W20 <= 0)
			{
				return 0;
			}
			return ServerData.Config.W20;
		}
	}

	private static int HARD_CAP_PER_FRAME
	{
		get
		{
			if (ServerData.Config.W21 <= 0)
			{
				return 0;
			}
			return ServerData.Config.W21;
		}
	}

	static LabRpcArbiter()
	{
		ConsumerCount = 4;
		_weights = new int[4] { 1, 3, 2, 4 };
		_active = new bool[ConsumerCount];
		_queues = new LinkedList<LabOp>[ConsumerCount];
		_sendTimestamps = new Queue<float>(64);
		_criticalRobinIdx = 0;
		_consecutiveSendFailures = 0;
		for (int i = 0; i < ConsumerCount; i++)
		{
			_queues[i] = new LinkedList<LabOp>();
		}
	}

	public static void Register(Consumer c)
	{
		_active[(int)c] = true;
	}

	public static void Unregister(Consumer c)
	{
		_active[(int)c] = false;
	}

	public static bool IsActive(Consumer c)
	{
		return _active[(int)c];
	}

	public static int ActiveCount()
	{
		int num = 0;
		for (int i = 0; i < ConsumerCount; i++)
		{
			if (_active[i])
			{
				num++;
			}
		}
		return num;
	}

	public static float GetBroadcastHz(Consumer c)
	{
		if (!_active[(int)c])
		{
			return 0f;
		}
		int num = 0;
		for (int i = 0; i < ConsumerCount; i++)
		{
			if (_active[i])
			{
				num += _weights[i];
			}
		}
		if (num <= 0)
		{
			return 0f;
		}
		return (float)HARD_CAP_PER_SEC * ((float)_weights[(int)c] / (float)num);
	}

	public static void Enqueue(LabOp op)
	{
		if (op == null || op.WriteSubMessage == null)
		{
			return;
		}
		LinkedList<LabOp> linkedList = _queues[(int)op.Consumer];
		if (linkedList.Count >= 400)
		{
			LinkedListNode<LabOp> linkedListNode = linkedList.Last;
			while (linkedListNode != null && linkedListNode.Value.Priority <= op.Priority)
			{
				linkedListNode = linkedListNode.Previous;
			}
			if (linkedListNode == null)
			{
				return;
			}
			linkedList.Remove(linkedListNode);
		}
		LinkedListNode<LabOp> linkedListNode2 = linkedList.First;
		while (linkedListNode2 != null && linkedListNode2.Value.Priority <= op.Priority)
		{
			linkedListNode2 = linkedListNode2.Next;
		}
		if (linkedListNode2 == null)
		{
			linkedList.AddLast(op);
		}
		else
		{
			linkedList.AddBefore(linkedListNode2, op);
		}
	}

	public static void FlushAll()
	{
		for (int i = 0; i < ConsumerCount; i++)
		{
			_queues[i].Clear();
		}
		_sendTimestamps.Clear();
	}

	public static void ResetState()
	{
		for (int i = 0; i < ConsumerCount; i++)
		{
			_queues[i].Clear();
			_active[i] = false;
		}
		_sendTimestamps.Clear();
		_criticalRobinIdx = 0;
		_consecutiveSendFailures = 0;
		HostOutboundBudget.Reset();
	}

	public static void Tick()
	{
		AmongUsClient instance = AmongUsClient.Instance;
		if ((Object)(object)instance == (Object)null || !((InnerNetClient)instance).AmConnected)
		{
			if (_consecutiveSendFailures != 0)
			{
				_consecutiveSendFailures = 0;
			}
			if ((Object)(object)instance == (Object)null || !((InnerNetClient)instance).AmConnected)
			{
				FlushAll();
			}
			return;
		}
		if (_consecutiveSendFailures >= 3)
		{
			FlushAll();
			return;
		}
		float time = Time.time;
		while (_sendTimestamps.Count > 0 && time - _sendTimestamps.Peek() > 1f)
		{
			_sendTimestamps.Dequeue();
		}
		int i = 0;
		if (TryDrainOneCritical())
		{
			i++;
		}
		int num = HARD_CAP_PER_FRAME + 2;
		for (; i < HARD_CAP_PER_FRAME; i++)
		{
			if (_sendTimestamps.Count >= HARD_CAP_PER_SEC)
			{
				break;
			}
			if (HostOutboundBudget.IsSaturated())
			{
				break;
			}
			if (num-- <= 0)
			{
				break;
			}
			int num2 = PickConsumerToDrain();
			if (num2 < 0 || !DrainBatchFromConsumer(num2))
			{
				break;
			}
		}
	}

	private static bool TryDrainOneCritical()
	{
		for (int i = 0; i < ConsumerCount; i++)
		{
			int num = (_criticalRobinIdx + i) % ConsumerCount;
			LinkedList<LabOp> linkedList = _queues[num];
			if (linkedList.Count != 0 && linkedList.First.Value.Priority == Priority.Critical && DrainBatchFromConsumer(num))
			{
				_criticalRobinIdx = (num + 1) % ConsumerCount;
				return true;
			}
		}
		return false;
	}

	private static int PickConsumerToDrain()
	{
		int result = -1;
		int num = -1;
		for (int i = 0; i < ConsumerCount; i++)
		{
			if (_queues[i].Count != 0)
			{
				int num2 = _weights[i];
				if (num2 > num)
				{
					num = num2;
					result = i;
				}
			}
		}
		return result;
	}

	private static bool DrainBatchFromConsumer(int consumerIdx)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		LinkedList<LabOp> linkedList = _queues[consumerIdx];
		if (linkedList.Count == 0)
		{
			return false;
		}
		LabOp value = linkedList.First.Value;
		linkedList.RemoveFirst();
		int num = (((int)value.SendOption == 1) ? 480 : 350);
		int targetClientId = value.TargetClientId;
		SendOption sendOption = value.SendOption;
		List<LabOp> list = new List<LabOp>(8);
		list.Add(value);
		int num2 = ((value.EstimatedBytes > 0) ? value.EstimatedBytes : 80);
		while (linkedList.Count > 0)
		{
			LabOp value2 = linkedList.First.Value;
			if ((value2.Priority == Priority.Critical && value.Priority != 0) || value2.TargetClientId != targetClientId || value2.SendOption != sendOption)
			{
				break;
			}
			int num3 = ((value2.EstimatedBytes > 0) ? value2.EstimatedBytes : 80);
			if (num2 + num3 > num)
			{
				break;
			}
			list.Add(value2);
			num2 += num3;
			linkedList.RemoveFirst();
		}
		int num4 = 0;
		int num5 = 0;
		try
		{
			if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected)
			{
				FlushAll();
				return false;
			}
			MessageWriter val = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(uint.MaxValue, (byte)0, sendOption, targetClientId);
			val.EndMessage();
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					list[i].WriteSubMessage(val);
					num4++;
				}
				catch (Exception ex)
				{
					num5++;
					Debug.LogWarning(Object.op_Implicit($"[LAB] writeSub threw c={consumerIdx} i={i}: {ex.Message}"));
				}
			}
			val.StartMessage((byte)2);
			val.WritePacked(uint.MaxValue);
			val.Write((byte)0);
			bool flag = true;
			try
			{
				((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val);
			}
			catch (Exception ex2)
			{
				flag = false;
				_consecutiveSendFailures++;
				Debug.LogWarning(Object.op_Implicit($"[LAB] FinishRpc threw c={consumerIdx} fails={_consecutiveSendFailures}: {ex2.Message}"));
			}
			if (flag)
			{
				_consecutiveSendFailures = 0;
			}
		}
		catch (Exception ex3)
		{
			_consecutiveSendFailures++;
			Debug.LogWarning(Object.op_Implicit($"[LAB] drain failed c={consumerIdx} n={list.Count} fails={_consecutiveSendFailures}: {ex3.Message}"));
			return false;
		}
		_sendTimestamps.Enqueue(Time.time);
		if (num4 > 0)
		{
			HostOutboundBudget.Record(num4);
		}
		Debug.Log(Object.op_Implicit($"[LAB] c={consumerIdx} batch={list.Count} invoked={num4} err={num5} bytes~{num2} tgt={targetClientId} opt={sendOption} prio={value.Priority} util={HostOutboundBudget.Utilization():F2}"));
		return true;
	}

	public static string GetStats()
	{
		int count = _queues[0].Count;
		int count2 = _queues[1].Count;
		int count3 = _queues[2].Count;
		int count4 = _queues[3].Count;
		return $"LAB q: GT={count} SC={count2} MP={count3} CL={count4} sent/s={_sendTimestamps.Count} active={ActiveCount()}";
	}
}
