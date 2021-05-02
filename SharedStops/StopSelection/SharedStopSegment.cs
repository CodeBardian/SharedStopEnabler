using ColossalFramework;
using SharedStopEnabler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedStopEnabler.StopSelection
{
    public class SharedStopSegment
    {
        public ushort m_segment { get; set; }

        public Dictionary<uint, List<ushort>> m_lanes = new Dictionary<uint, List<ushort>>();

        [Flags]
        public enum SharedStopTypes
        {
            None = 0,
            Bus = 1,
            Tram = 2,
            TouristBus = 4,
            Trolleybus = 8,
        }

        public SharedStopSegment(ushort segment, ushort line, uint lane)
        {
            m_segment = segment;
            m_lanes.Add(lane, new List<ushort>() { line });
        }
    }
}
