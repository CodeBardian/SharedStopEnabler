using SharedStopEnabler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedStopEnabler.StopSelection
{
    class SharedStopSegment
    {
        public ushort m_segment { get; set; }
        public SharedStopsTool.SharedStopTypes m_sharedStopTypes { get; set; }

        public List<ushort> m_lines = new List<ushort>();

    public SharedStopSegment(ushort segment, SharedStopsTool.SharedStopTypes sharedStopTypes, ushort line)
        {
            Log.Debug($"create sharedsegment: {segment}, {sharedStopTypes}, {line}");
            m_segment = segment;
            m_sharedStopTypes = sharedStopTypes;
            m_lines.Add(line);
        }
    }

    //TODO: Set props/ init Props
}
