using ColossalFramework;
using HarmonyLib;
using SharedStopEnabler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SharedStopEnabler.RedirectionFramework.Attributes;
using UnityEngine;

namespace SharedStopEnabler.StopSelection.Patch
{
	[TargetType(typeof(RoadAI))]
	class RoadAIDetour : RoadAI
	{
		[RedirectMethod]
		public override void UpdateSegmentFlags(ushort segmentID, ref NetSegment data)
		{
			// calls base.UpdateSegmentFlags(segmentID, ref data);
			//object[] args = new object[] { segmentID, data };
			//object result = typeof(RoadBaseAI).GetMethod("UpdateSegmentFlags", new[] { typeof(ushort), typeof(NetSegment).MakeByRefType() }).Invoke(this, args);
			//if ((bool)result)
			//	data = (NetSegment)args[1];
			var ptr = typeof(RoadBaseAI).GetMethod("UpdateSegmentFlags", new[] { typeof(ushort), typeof(NetSegment).MakeByRefType() }).MethodHandle.GetFunctionPointer();
			var baseUpdate = (Action<ushort, NetSegment>)Activator.CreateInstance(typeof(Action<ushort, NetSegment>), this, ptr);
			baseUpdate(segmentID, data);

			var oldflags = data.m_flags;
			NetSegment.Flags flags = data.m_flags & ~(NetSegment.Flags.StopRight | NetSegment.Flags.StopLeft | NetSegment.Flags.StopRight2 | NetSegment.Flags.StopLeft2);
			if (m_info.m_lanes != null)
			{
				NetManager instance = Singleton<NetManager>.instance;
				bool flag = (data.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None;
				uint num = instance.m_segments.m_buffer[(int)segmentID].m_lanes;
				int num2 = 0;
				while (num2 < m_info.m_lanes.Length && num != 0U)
				{
					NetLane.Flags flags2 = (NetLane.Flags)instance.m_lanes.m_buffer[(int)((UIntPtr)num)].m_flags;
					if ((flags2 & NetLane.Flags.Stop) != NetLane.Flags.None)
					{
						if (m_info.m_lanes[num2].m_position < 0f != flag)
						{
							flags |= NetSegment.Flags.StopLeft;
						}
						else
						{
							flags |= NetSegment.Flags.StopRight;
						}
					}
					else if ((flags2 & NetLane.Flags.Stop2) != NetLane.Flags.None)
					{
						if (m_info.m_lanes[num2].m_position < 0f != flag)
						{
							flags |= NetSegment.Flags.StopLeft2;
						}
						else
						{
							flags |= NetSegment.Flags.StopRight2;
						}
					}
					num = instance.m_lanes.m_buffer[(int)((UIntPtr)num)].m_nextLane;
					num2++;
				}
			}
			if (oldflags != flags && data.IsSharedStop((int)segmentID))
			{
				var index = Singleton<SharedStopsTool>.instance.sharedStopSegments.FindIndex(s => s.m_segment == segmentID);
				var inverted = (flags & NetSegment.Flags.Invert) == NetSegment.Flags.Invert;
				Singleton<SharedStopsTool>.instance.sharedStopSegments[index].UpdateStopFlags(inverted, out NetSegment.Flags stopflags);
				data.m_flags = flags;
				data.m_flags |= stopflags;
				Log.Debug($"oldflags {oldflags} newflagsSS {data.m_flags}");
				return;
			}
			Log.Debug($"oldflags {oldflags} newflags {flags}");
			data.m_flags = flags;
		}
	}

	//[HarmonyPatch(typeof(RoadAI), "UpdateSegmentFlags")]
	//class TransportLineAIPatch1
	//{
	//	static bool Prefix(ref NetSegment __state, ushort segmentID, ref NetSegment data)
	//	{
	//		__state = data;
	//		//if (Singleton<NetManager>.instance.m_segments.m_buffer[segmentID].IsSharedStop(segmentID))
	//		//{
	//		//	return false;
	//		//}
	//		Log.Debug($"called update segment flags {data.m_flags}");
	//		return true;
	//	}

	//	static void Postfix(ref NetSegment __state)
	//	{
	//		Log.Debug($"called update segment flags finished {__state.m_flags}");
	//	}
	//}
}
