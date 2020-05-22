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
        public SharedStopsTool.SharedStopTypes m_sharedStopTypesForward { get; set; }
        public SharedStopsTool.SharedStopTypes m_sharedStopTypesBackward { get; set; }
        public NetInfo.Direction m_directionFlags { get; set; }
        public List<ushort> m_lines = new List<ushort>();

        public SharedStopSegment(ushort segment, SharedStopsTool.SharedStopTypes sharedStopTypes, ushort line, NetInfo.Direction direction)
        {
            m_segment = segment;
            m_directionFlags = direction;
            m_lines.Add(line);
            if (direction == NetInfo.Direction.Forward) m_sharedStopTypesForward = sharedStopTypes;
            else if (direction == NetInfo.Direction.Backward) m_sharedStopTypesBackward = sharedStopTypes;
            else Log.Warning("no sharedstoptypes set");
        }
    }

    //TODO: Set props/ init Props
}
