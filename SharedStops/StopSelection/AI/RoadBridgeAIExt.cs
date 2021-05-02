using ColossalFramework;
using SharedStopEnabler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedStopEnabler.StopSelection.AI
{
	static class RoadBridgeAIExt
	{
		public static void UpdateSegmentStopFlags(this RoadBridgeAI roadbridge, ushort segmentID, ref NetSegment data)
		{
			roadbridge.UpdateSegmentFlags(segmentID, ref data);
			var oldflags = data.m_flags;
			NetSegment.Flags flags = data.m_flags & ~(NetSegment.Flags.StopRight | NetSegment.Flags.StopLeft | NetSegment.Flags.StopRight2 | NetSegment.Flags.StopLeft2);
			if (roadbridge.m_info.m_lanes != null)
			{
				NetManager instance = Singleton<NetManager>.instance;
				bool flag = (data.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None;
				uint num = instance.m_segments.m_buffer[(int)segmentID].m_lanes;
				int num2 = 0;
				while (num2 < roadbridge.m_info.m_lanes.Length && num != 0U)
				{
					NetLane.Flags flags2 = (NetLane.Flags)instance.m_lanes.m_buffer[(int)((UIntPtr)num)].m_flags;
					if ((flags2 & NetLane.Flags.Stop) != NetLane.Flags.None)
					{
						if (roadbridge.m_info.m_lanes[num2].m_position < 0f != flag)
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
						if (roadbridge.m_info.m_lanes[num2].m_position < 0f != flag)
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
			Log.Debug($"oldflags {oldflags} newflags {flags} on segment {segmentID} bridge");
			data.m_flags = flags;
		}
	}
}

