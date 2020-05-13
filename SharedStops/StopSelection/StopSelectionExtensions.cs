using ColossalFramework;
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
            return (Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.StopBoth) != NetSegment.Flags.None;
        }
    }
}
