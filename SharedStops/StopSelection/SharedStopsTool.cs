using ColossalFramework;
using ColossalFramework.Math;
using SharedStopEnabler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using SharedStopEnabler.StopSelection.AI;
using ColossalFramework.UI;

namespace SharedStopEnabler.StopSelection
{
    class SharedStopsTool : MonoBehaviour
    {

        public List<SharedStopSegment> sharedStopSegments;

        public void Start()
        {
            try
            {
                sharedStopSegments = new List<SharedStopSegment>();
                StopsUtil.EnableElevatedStops();

                StopsUtil.InitLaneProps("Tram Stop");
                InitStopTypes();
                RecalculateSharedStopSegments();

                Log.Info($"successful startup");
            }
            catch (Exception e)
            {
                Log.Error($"Failed on startup {e}");
            }
        }

        public void OnDestroy()
        {
            try
            {
                sharedStopSegments.Clear();
                Log.Info($"on destroy finished");
            }
            catch (Exception e)
            {
                Log.Error($"Failed on destroy {e}");
            }
        }

        private void RecalculateSharedStopSegments()
        {
            foreach (TransportLine line in Singleton<TransportManager>.instance.m_lines.m_buffer)
            {
                if (line.Info.m_class.m_subService != ItemClass.SubService.PublicTransportBus && line.Info.m_class.m_subService != ItemClass.SubService.PublicTransportTram &&
                    line.Info.m_class.m_subService != ItemClass.SubService.PublicTransportTrolleybus && line.Info.m_class.name != "Sightseeing Bus Line") continue;

                ushort lineID = Singleton<NetManager>.instance.m_nodes.m_buffer[line.m_stops].m_transportLine;

                if (lineID == 0) continue;
                ushort stops = line.m_stops;
                if (stops == 0) continue;
                for (int i = 0; i < line.CountStops(lineID); i++)
                {
                    stops = TransportLine.GetNextStop(stops);
                    uint lane = Singleton<NetManager>.instance.m_nodes.m_buffer[stops].m_lane;
                    ushort segment = Singleton<NetManager>.instance.m_lanes.m_buffer[lane].m_segment;
                    if (lane != 0 && segment != 0)
                    {
                        Log.Debug($"stop {stops} on lane {lane} on segment {segment}");
                        Singleton<SharedStopsTool>.instance.AddSharedStop(segment, lineID, lane);
                    }
                    if (stops == line.m_stops) break;
                }
                Log.Debug($"Stops found {sharedStopSegments.Count}");
            } 
        }

        public void AddSharedStop(ushort segment, ushort line, uint lane)
        {
            if (sharedStopSegments.Any(s => s.m_segment == segment))
            {
                var sharedStopSegment = sharedStopSegments[sharedStopSegments.FindIndex(s => s.m_segment == segment)];
                if (sharedStopSegment.m_lanes.Keys.Contains(lane))
                {
                    sharedStopSegment.m_lanes[lane].Add(line);
                    Log.Debug($"add to existing segment {segment}, lane: {lane}, line:{line}");
                }
                else sharedStopSegment.m_lanes.Add(lane, new List<ushort>() { line });
            }
            else 
            {
                var newSegment = new SharedStopSegment(segment, line, lane);
                sharedStopSegments.Add(newSegment);
                Log.Debug($"add sharedsegment {segment}, {sharedStopSegments.Count}, lane: {lane}");
            }
            NetAI roadAi = Singleton<NetManager>.instance.m_segments.m_buffer[segment].Info.m_netAI;
            if (roadAi is RoadBridgeAI roadBridgeAI)
            {
                roadBridgeAI.UpdateSegmentStopFlags(segment, ref Singleton<NetManager>.instance.m_segments.m_buffer[segment]);
            }
        }

        public bool RemoveSharedStop(ushort segment, ushort line, uint lane)
        {
            if (sharedStopSegments.Any(s => s.m_segment == segment))
            {
                SharedStopSegment sharedStopSegment = sharedStopSegments[sharedStopSegments.FindIndex(s => s.m_segment == segment)];
                if (sharedStopSegment.m_lanes.Keys.Contains(lane))
                {
                    sharedStopSegment.m_lanes[lane].Remove(line);
                    Log.Debug($"removed line {line} from sharedsegment on lane {lane}");
                    if (sharedStopSegment.m_lanes[lane].Count == 0)
                    {
                        sharedStopSegment.m_lanes.Remove(lane);
                        Log.Debug($"removed lane {lane} from sharedsegment");
                    }
                }
                if (sharedStopSegment.m_lanes.Count == 0)
                {
                    Singleton<NetManager>.instance.m_segments.m_buffer[sharedStopSegment.m_segment].m_flags &= ~NetSegment.Flags.StopAll;
                    sharedStopSegments.Remove(sharedStopSegment);
                    Log.Debug($"removed sharedsegment {segment}");
                }
                return true;
            }
            Log.Debug($"found no sharedsegment to remove");
            return false;
        }

        private void InitStopTypes()
        {
            NetInfo[] networks = Resources.FindObjectsOfTypeAll<NetInfo>();

            foreach (NetInfo segment in networks)
            {
                if (!segment.m_hasPedestrianLanes) continue;
                for (int i = 1; i < segment.m_lanes.Length - 2; i++)
                {
                    uint index = (uint)segment.m_sortedLanes[i];
                    if (IsValidLane(segment, (uint)i))
                    {
                        segment.m_lanes[index].m_stopType |= VehicleInfo.VehicleType.Car;
                    }
                }
            }
        }

        private bool IsValidLane(NetInfo segment, uint laneIndex) //check left and right lanes
        {
            uint index = (uint)segment.m_sortedLanes[laneIndex];
            if ((segment.m_lanes[index].m_vehicleType == VehicleInfo.VehicleType.None) && ((segment.m_lanes[index].m_stopType & VehicleInfo.VehicleType.Car) != VehicleInfo.VehicleType.Car))
            {
                int index1 = segment.m_sortedLanes[laneIndex + 1];
                int index2 = segment.m_sortedLanes[laneIndex - 1];
                if (((segment.m_lanes[index1].m_vehicleType & VehicleInfo.VehicleType.Car) != VehicleInfo.VehicleType.Car) || ((segment.m_lanes[index2].m_vehicleType & VehicleInfo.VehicleType.Car) != VehicleInfo.VehicleType.Car))
                {
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
