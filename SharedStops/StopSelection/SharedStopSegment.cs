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
        public SharedStopSegment.SharedStopTypes m_sharedStopTypesForward { get; set; }
        public SharedStopSegment.SharedStopTypes m_sharedStopTypesBackward { get; set; }

        public Dictionary<ushort,NetInfo.Direction> m_lines = new Dictionary<ushort, NetInfo.Direction>();

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
            m_lines.Add(line, direction);
            if (direction == NetInfo.Direction.Forward) m_sharedStopTypesForward = sharedStopTypes;
            else if (direction == NetInfo.Direction.Backward) m_sharedStopTypesBackward = sharedStopTypes;
            else Log.Warning("no sharedstoptypes set");
        }

        //public void UpdateProps(NetInfo.Direction direction)
        //{
        //    var segment = Singleton<NetManager>.instance.m_segments.m_buffer[m_segment];

        //    if (segment.Info == null || segment.Info.m_lanes == null || !segment.Info.m_hasPedestrianLanes) return;

        //    //var segmentInfo = Clone.DeepClone<NetInfo>(segment.Info);

        //    for (int i = 0; i < segment.Info.m_lanes.Length; i++)
        //    {
        //        NetInfo.Lane lane = segment.Info.m_lanes[i];
        //        if (lane == null || lane.m_laneType != NetInfo.LaneType.Pedestrian || lane.m_laneProps == null || lane.m_laneProps.m_props == null) continue;

        //        segment.GetClosestLane(i, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.All, out int laneindex, out uint laneID);
        //        if (segment.Info.m_lanes[laneindex].m_direction != direction) continue;

        //        foreach (var laneProp in lane.m_laneProps.m_props)
        //        {
        //            if (laneProp == null && laneProp.m_prop == null) continue;

        //            if (laneProp.m_prop.name == "Tram Stop")
        //            {
        //                laneProp.m_flagsForbidden = NetLane.Flags.Stop;
        //            }
        //            Log.Debug($"lane {i} closest lane {laneindex}");
        //            if (segment.Info.m_lanes[laneindex].m_direction == NetInfo.Direction.Forward)
        //            {
        //                if ((m_sharedStopTypesForward & SharedStopTypes.Bus) == SharedStopTypes.Bus)
        //                {
        //                    if (laneProp.m_prop.name.StartsWith("Sightseeing") || laneProp.m_prop.name.StartsWith("Trolley"))
        //                    {
        //                        laneProp.m_flagsForbidden = NetLane.Flags.Stop;
        //                        laneProp.m_flagsRequired = ~NetLane.Flags.None;
        //                    }
        //                    else if (laneProp.m_prop.name.StartsWith("Bus"))
        //                    {
        //                        laneProp.m_flagsForbidden = NetLane.Flags.None;
        //                        laneProp.m_flagsRequired = NetLane.Flags.Stop;
        //                    }
        //                }
        //                else if ((m_sharedStopTypesForward & SharedStopTypes.TouristBus) == SharedStopTypes.TouristBus)
        //                {
        //                    if (laneProp.m_prop.name.StartsWith("Bus") || laneProp.m_prop.name.StartsWith("Trolley"))
        //                    {
        //                        laneProp.m_flagsForbidden = NetLane.Flags.Stop;
        //                        laneProp.m_flagsRequired = ~NetLane.Flags.None;
        //                    }
        //                    else if (laneProp.m_prop.name.StartsWith("Sightseeing"))
        //                    {
        //                        laneProp.m_flagsForbidden = NetLane.Flags.None;
        //                        laneProp.m_flagsRequired = NetLane.Flags.Stop;
        //                    }
        //                }
        //                else if ((m_sharedStopTypesForward & SharedStopTypes.Trolleybus) == SharedStopTypes.Trolleybus)
        //                {
        //                    if (laneProp.m_prop.name.StartsWith("Bus") || laneProp.m_prop.name.StartsWith("Sightseeing"))
        //                    {
        //                        laneProp.m_flagsForbidden = NetLane.Flags.Stop;
        //                        laneProp.m_flagsRequired = ~NetLane.Flags.None;
        //                    }
        //                    else if (laneProp.m_prop.name.StartsWith("Trolley"))
        //                    {
        //                        laneProp.m_flagsForbidden = NetLane.Flags.None;
        //                        laneProp.m_flagsRequired = NetLane.Flags.Stop;
        //                    }
        //                }
        //            }
        //            else if (segment.Info.m_lanes[laneindex].m_direction == NetInfo.Direction.Backward)
        //            {
        //                if ((m_sharedStopTypesBackward & SharedStopTypes.Bus) == SharedStopTypes.Bus)
        //                {
        //                    if (laneProp.m_prop.name.StartsWith("Sightseeing") || laneProp.m_prop.name.StartsWith("Trolley"))
        //                    {
        //                        laneProp.m_flagsForbidden = NetLane.Flags.Stop;
        //                        laneProp.m_flagsRequired = ~NetLane.Flags.None;
        //                    }
        //                    else if (laneProp.m_prop.name.StartsWith("Bus"))
        //                    {
        //                        laneProp.m_flagsForbidden = NetLane.Flags.None;
        //                        laneProp.m_flagsRequired = NetLane.Flags.Stop;
        //                    }
        //                }
        //                else if ((m_sharedStopTypesBackward & SharedStopTypes.TouristBus) == SharedStopTypes.TouristBus)
        //                {
        //                    if (laneProp.m_prop.name.StartsWith("Bus") || laneProp.m_prop.name.StartsWith("Trolley"))
        //                    {
        //                        laneProp.m_flagsForbidden = NetLane.Flags.Stop;
        //                        laneProp.m_flagsRequired = ~NetLane.Flags.None;
        //                    }
        //                    else if (laneProp.m_prop.name.StartsWith("Sightseeing"))
        //                    {
        //                        laneProp.m_flagsForbidden = NetLane.Flags.None;
        //                        laneProp.m_flagsRequired = NetLane.Flags.Stop;
        //                    }
        //                }
        //                else if ((m_sharedStopTypesBackward & SharedStopTypes.Trolleybus) == SharedStopTypes.Trolleybus)
        //                {
        //                    if (laneProp.m_prop.name.StartsWith("Bus") || laneProp.m_prop.name.StartsWith("Sightseeing"))
        //                    {
        //                        laneProp.m_flagsForbidden = NetLane.Flags.Stop;
        //                        laneProp.m_flagsRequired = ~NetLane.Flags.None;
        //                    }
        //                    else if (laneProp.m_prop.name.StartsWith("Trolley"))
        //                    {
        //                        laneProp.m_flagsForbidden = NetLane.Flags.None;
        //                        laneProp.m_flagsRequired = NetLane.Flags.Stop;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    //segment.Info = segmentInfo;
        //}
        //internal void InitProps()
        //{
        //    var segment = Singleton<NetManager>.instance.m_segments.m_buffer[m_segment];

        //    if (segment.Info == null || segment.Info.m_lanes == null || !segment.Info.m_hasPedestrianLanes) return;

        //    foreach (NetInfo.Lane lane in segment.Info.m_lanes)
        //    {
        //        if (lane == null || lane.m_laneType != NetInfo.LaneType.Pedestrian || lane.m_laneProps == null || lane.m_laneProps.m_props == null) continue;

        //        foreach (NetLaneProps.Prop laneProp in lane.m_laneProps.m_props)
        //        {
        //            if (laneProp == null && laneProp.m_prop == null) continue;

        //            if (laneProp.m_prop.name.StartsWith("Bus") || laneProp.m_prop.name.StartsWith("Sightseeing") || laneProp.m_prop.name.StartsWith("Trolley"))
        //            {
        //                laneProp.m_flagsForbidden = NetLane.Flags.None;
        //                laneProp.m_flagsRequired = NetLane.Flags.Stops;
        //            }
        //        }
        //    }
        //}

        public void UpdateStopFlags(bool inverted, out NetSegment.Flags stopflags)
        {
            stopflags = NetSegment.Flags.None;
            Log.Debug($"forward {m_sharedStopTypesForward} backward {m_sharedStopTypesBackward} on segment {m_segment}");
            if ((m_sharedStopTypesBackward & SharedStopTypes.Bus) == SharedStopTypes.Bus || (m_sharedStopTypesBackward & SharedStopTypes.Trolleybus) == SharedStopTypes.Trolleybus || (m_sharedStopTypesBackward & SharedStopTypes.TouristBus) == SharedStopTypes.TouristBus)
            {
                if (inverted) stopflags |= NetSegment.Flags.StopRight;
                else stopflags |= NetSegment.Flags.StopLeft;

            }
            if ((m_sharedStopTypesForward & SharedStopTypes.Bus) == SharedStopTypes.Bus || (m_sharedStopTypesForward & SharedStopTypes.Trolleybus) == SharedStopTypes.Trolleybus || (m_sharedStopTypesForward & SharedStopTypes.TouristBus) == SharedStopTypes.TouristBus)
            {
                if (inverted) stopflags |= NetSegment.Flags.StopLeft;
                else stopflags |= NetSegment.Flags.StopRight;
            }
            if ((m_sharedStopTypesBackward & SharedStopTypes.Tram) == SharedStopTypes.Tram)
            {
                if (inverted) stopflags |= NetSegment.Flags.StopRight2;
                else stopflags |= NetSegment.Flags.StopLeft2;
            }
            if ((m_sharedStopTypesForward & SharedStopTypes.Tram) == SharedStopTypes.Tram)
            {
                if (inverted) stopflags |= NetSegment.Flags.StopLeft2;
                else stopflags |= NetSegment.Flags.StopRight2;
            }
            Log.Debug($"stopflag {stopflags}");
        }
    }
    //TODO: Set props/ init Props
}
