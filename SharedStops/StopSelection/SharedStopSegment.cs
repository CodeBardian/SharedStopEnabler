using ColossalFramework;
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
        public SharedStopSegment.SharedStopTypes m_sharedStopTypesForward { get; set; }
        public SharedStopSegment.SharedStopTypes m_sharedStopTypesBackward { get; set; }
        public NetInfo.Direction m_directionFlags { get; set; }
        public List<ushort> m_lines = new List<ushort>();

        [Flags]
        public enum SharedStopTypes
        {
            None = 0,
            Bus = 1,
            Tram = 2,
            TouristBus = 4,
            Trolleybus = 8,
        }

        public SharedStopSegment(ushort segment, SharedStopSegment.SharedStopTypes sharedStopTypes, ushort line, NetInfo.Direction direction)
        {
            m_segment = segment;
            m_directionFlags = direction;
            m_lines.Add(line);
            if (direction == NetInfo.Direction.Forward) m_sharedStopTypesForward = sharedStopTypes;
            else if (direction == NetInfo.Direction.Backward) m_sharedStopTypesBackward = sharedStopTypes;
            else Log.Warning("no sharedstoptypes set");
            //SetProps(m_sharedStopTypesForward, m_sharedStopTypesBackward);
        }

        public void UpdateProps()
        {
            var segment = Singleton<NetManager>.instance.m_segments.m_buffer[m_segment];

            if (segment.Info == null || segment.Info.m_lanes == null || !segment.Info.m_hasPedestrianLanes) return;

            foreach (NetInfo.Lane lane in segment.Info.m_lanes)
            {
                if (lane == null || lane.m_laneType != NetInfo.LaneType.Pedestrian || lane.m_laneProps == null || lane.m_laneProps.m_props == null) continue;

                foreach (NetLaneProps.Prop laneProp in lane.m_laneProps.m_props)
                {
                    if (laneProp == null && laneProp.m_prop == null) continue;

                    if (laneProp.m_prop.name == "Tram Stop")
                    {
                        laneProp.m_flagsForbidden = NetLane.Flags.Stop;
                    }
                }
            }
        }

        public void UpdateStopFlags(out NetSegment.Flags stopflags)
        {
            stopflags = NetSegment.Flags.None;
            if ((m_sharedStopTypesBackward & SharedStopTypes.Bus) == SharedStopTypes.Bus || (m_sharedStopTypesBackward & SharedStopTypes.Trolleybus) == SharedStopTypes.Trolleybus || (m_sharedStopTypesBackward & SharedStopTypes.TouristBus) == SharedStopTypes.TouristBus)
            {
                stopflags |= NetSegment.Flags.StopLeft;
            }
            if ((m_sharedStopTypesForward & SharedStopTypes.Bus) == SharedStopTypes.Bus || (m_sharedStopTypesForward & SharedStopTypes.Trolleybus) == SharedStopTypes.Trolleybus || (m_sharedStopTypesForward & SharedStopTypes.TouristBus) == SharedStopTypes.TouristBus)
            {
                stopflags |= NetSegment.Flags.StopRight;
            }
            if ((m_sharedStopTypesBackward & SharedStopTypes.Tram) == SharedStopTypes.Tram)
            {
                stopflags |= NetSegment.Flags.StopLeft2;
            }
            if ((m_sharedStopTypesForward & SharedStopTypes.Tram) == SharedStopTypes.Tram)
            {
                stopflags |= NetSegment.Flags.StopRight2;
            }
        }
    }

    //TODO: Set props/ init Props
}
