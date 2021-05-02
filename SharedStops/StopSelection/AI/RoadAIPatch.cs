using ColossalFramework;
using HarmonyLib;
using SharedStopEnabler.Util;

namespace SharedStopEnabler.StopSelection.Patch
{
	[HarmonyPatch(typeof(RoadAI), "UpdateSegmentFlags")]
	class RoadAIPatch_UpdateSegmentFlags
	{
		static void Postfix(RoadAI __instance, ushort segmentID, ref NetSegment data)
        {
			if (!data.IsSharedStopSegment(segmentID)) return;

			NetSegment.Flags flags = data.m_flags & ~(NetSegment.Flags.StopRight | NetSegment.Flags.StopLeft | NetSegment.Flags.StopRight2 | NetSegment.Flags.StopLeft2);
			if (__instance.m_info.m_lanes != null)
			{
				NetManager instance = Singleton<NetManager>.instance;
				bool flag = (data.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None;
				uint num = instance.m_segments.m_buffer[(int)segmentID].m_lanes;
				int num2 = 0;
				while (num2 < __instance.m_info.m_lanes.Length && num != 0U)
				{
					NetLane.Flags flags2 = (NetLane.Flags)instance.m_lanes.m_buffer[num].m_flags;
					if ((flags2 & NetLane.Flags.Stop) != NetLane.Flags.None)
					{
						if (__instance.m_info.m_lanes[num2].m_position < 0f != flag)
						{
							flags |= NetSegment.Flags.StopLeft;
						}
						else
						{
							flags |= NetSegment.Flags.StopRight;
						}
					}
					if ((flags2 & NetLane.Flags.Stop2) != NetLane.Flags.None)
					{
						if (__instance.m_info.m_lanes[num2].m_position < 0f != flag)
						{
							flags |= NetSegment.Flags.StopLeft2;
						}
						else
						{
							flags |= NetSegment.Flags.StopRight2;
						}
					}
					num = instance.m_lanes.m_buffer[num].m_nextLane;
					num2++;
				}
			}
			data.m_flags = flags;
			Log.Debug($"newflags {data.m_flags}");
		}		
	}
}
