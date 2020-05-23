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

        public static bool IsSharedStop(this NetSegment netSegment, int segment)  //doesnt work for elevated roads, they dont have stoptypes, due to vanilla elevated stop restriction
        {
            return Singleton<SharedStopsTool>.instance.sharedStopSegments.Any(s => s.m_segment == segment); 
        }
    }
}
