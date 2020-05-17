using ColossalFramework;
using SharedStopEnabler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SharedStopEnabler.StopSelection
{
    public static class StopSelectionExtensions
    {
        public static bool FlagSet(this TransportLine transportLine, TransportLine.Flags flag)
        {
            return (transportLine.m_flags & flag) == flag;
        }

        public static bool HasStops(this NetSegment netSegment, int segment)
        {
            return (Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.StopAll) != NetSegment.Flags.None;
        }

        public static bool HasSharedStop(this NetSegment netSegment, int segment, NetLane.Flags stopFlag)  //doesnt work for elevated roads, they dont have stoptypes, due to vanilla elevated stop restriction
        {
            bool hasStops = netSegment.HasStops(segment);
            NetSegment.Flags existingFlags = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment].m_flags;
            Log.Debug($"hasStops: {hasStops}, existingflag: {existingFlags}, stopflag: {(NetSegment.Flags)stopFlag}");
            return hasStops && (existingFlags & (NetSegment.Flags)stopFlag) != (NetSegment.Flags)stopFlag;
        }
    }
}
