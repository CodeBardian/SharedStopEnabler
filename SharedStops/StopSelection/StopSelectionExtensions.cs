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

        public static bool IsSharedStopSegment(this NetSegment netSegment, int segment)
        {
            if (Singleton<SharedStopsTool>.instance.sharedStopSegments == null)
                return false;
            return Singleton<SharedStopsTool>.instance.sharedStopSegments.Any(s => s.m_segment == segment); 
        }

        public static List<SharedStopSegment> FindSharedStopsByLine(ushort line)  
        {
            return Singleton<SharedStopsTool>.instance.sharedStopSegments.Where(s => s.m_lines.ContainsKey(line)).ToList();
        }

        public static bool IsSharedStopTransport(this TransportInfo.TransportType transportType)
        {
            return transportType == TransportInfo.TransportType.Bus || transportType == TransportInfo.TransportType.Tram || transportType == TransportInfo.TransportType.TouristBus || transportType == TransportInfo.TransportType.Trolleybus;
        }
    }
}
