using ColossalFramework;
using HarmonyLib;
using SharedStopEnabler.Util;

namespace SharedStopEnabler.StopSelection.Patch
{
	[HarmonyPatch(typeof(RoadAI), "UpdateSegmentFlags")]
	class RoadAIPatch_UpdateSegmentFlags
	{
		static bool Prefix(out NetSegment.Flags __state, ushort segmentID, ref NetSegment data)
        {
			__state = data.m_flags;
			return true;
        }

		static void Postfix(NetSegment.Flags __state, ushort segmentID, ref NetSegment data)
        {
			if (__state != data.m_flags && data.IsSharedStopSegment((int)segmentID))
            {
                var index = Singleton<SharedStopsTool>.instance.sharedStopSegments.FindIndex(s => s.m_segment == segmentID);
                var inverted = (data.m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.Invert;
                Singleton<SharedStopsTool>.instance.sharedStopSegments[index].UpdateStopFlags(inverted, out NetSegment.Flags stopflags);
                data.m_flags |= stopflags;
                Log.Debug($"oldflags {__state} flags {data.m_flags} newflagsSharedStop {data.m_flags}");
            }
        }		
	}
}
