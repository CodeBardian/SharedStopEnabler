﻿using ColossalFramework;
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
                Log.Debug($"Checking line {lineID}, {line.Info.m_class.m_subService}, {line.m_stops}, {line.CountStops(lineID)}");

                if (lineID == 0) continue;
                ushort stops = line.m_stops;
                if (stops == 0) continue;
                for (int i = 0; i < line.CountStops(lineID); i++)
                {
                    stops = TransportLine.GetNextStop(stops);
                    uint lane = Singleton<NetManager>.instance.m_nodes.m_buffer[stops].m_lane;
                    Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[stops].m_position;
                    ushort segment = Singleton<NetManager>.instance.m_lanes.m_buffer[lane].m_segment;
                    if (lane != 0 && segment != 0)
                    {
                        Log.Debug($"stop {stops} on lane {lane} on segment {segment}");
                        if (Singleton<NetManager>.instance.m_segments.m_buffer[segment].GetClosestLanePosition(position, NetInfo.LaneType.Vehicle, line.Info.m_vehicleType, out _, out _, out int laneindex, out _))
                        {
                            NetInfo.Direction direction = Singleton<NetManager>.instance.m_segments.m_buffer[segment].Info.m_lanes[laneindex].m_direction;
                            Singleton<SharedStopsTool>.instance.AddSharedStop(segment, line.Info.m_transportType.Convert(), lineID, direction);
                        }
                        NetSegment data = Singleton<NetManager>.instance.m_segments.m_buffer[segment];
                        int index = Singleton<SharedStopsTool>.instance.sharedStopSegments.FindIndex(s => s.m_segment == segment);
                        if (index != -1)
                        {
                            var inverted = (data.m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.Invert;
                            Singleton<SharedStopsTool>.instance.sharedStopSegments[index].UpdateStopFlags(inverted, out NetSegment.Flags stopflags);
                            Log.Debug($"present flags: {data.m_flags}");
                            data.m_flags |= stopflags;
                            Log.Debug($"new flags: {data.m_flags}");
                        }
                        //RoadAI roadAi = Singleton<NetManager>.instance.m_segments.m_buffer[segment].Info.m_netAI as RoadAI;
                        //roadAi?.UpdateSegmentFlags(segment, ref data);
                    }
                    if (stops == line.m_stops) break;
                }
                Log.Debug($"Stops found {sharedStopSegments.Count}");
            } 
        }

        public void AddSharedStop(ushort segment, SharedStopSegment.SharedStopTypes sharedStopTypes, ushort line, NetInfo.Direction direction)
        {
            Log.Debug($"trying to add line to sharedsegment {segment}, {sharedStopTypes}, {line}, {direction}");
            if (sharedStopSegments.Any(s => s.m_segment == segment))
            {
                var sharedStopSegment = sharedStopSegments[sharedStopSegments.FindIndex(s => s.m_segment == segment)];
                if (sharedStopSegment.m_lines.Keys.Contains(line))
                {
                    sharedStopSegment.m_lines[line] |= direction;
                    Log.Debug($"add to existing segment {segment}, {sharedStopTypes}, {line}, {direction}");
                }
                else sharedStopSegment.m_lines.Add(line, direction);
                if (direction == NetInfo.Direction.Forward) sharedStopSegment.m_sharedStopTypesForward |= sharedStopTypes;
                else if (direction == NetInfo.Direction.Backward) sharedStopSegment.m_sharedStopTypesBackward |= sharedStopTypes;
                Log.Debug($"forward {sharedStopSegment.m_sharedStopTypesForward}, backward{sharedStopSegment.m_sharedStopTypesBackward}");
                //sharedStopSegment.UpdateProps(direction);
            }
            else 
            {
                var newSegment = new SharedStopSegment(segment, sharedStopTypes, line, direction);
                //newSegment.UpdateProps(direction);
                sharedStopSegments.Add(newSegment);
                Log.Debug($"add sharedsegment {segment}, {sharedStopSegments.Count}, direction: {direction}");
            }
            NetAI roadAi = Singleton<NetManager>.instance.m_segments.m_buffer[segment].Info.m_netAI;
            Log.Debug($"netAi {roadAi}");
            if (roadAi is RoadBridgeAI roadBridgeAI)
            {
                roadBridgeAI.UpdateSegmentStopFlags(segment, ref Singleton<NetManager>.instance.m_segments.m_buffer[segment]);
            }
        }

        public bool RemoveSharedStop(ushort segment, SharedStopSegment.SharedStopTypes sharedStopTypes, ushort line, NetInfo.Direction direction)
        {
            if (sharedStopSegments.Any(s => s.m_segment == segment))
            {
                SharedStopSegment sharedStopSegment = sharedStopSegments[sharedStopSegments.FindIndex(s => s.m_segment == segment)];
                if (sharedStopSegment.m_lines.Keys.Contains(line)) //TODO: remove unnecessary checks
                {
                    sharedStopSegment.m_lines[line] &= ~direction;
                    if (sharedStopSegment.m_lines[line] == NetInfo.Direction.None)
                    {
                        sharedStopSegment.m_lines.Remove(line);
                    }
                    if (!sharedStopSegment.m_lines.Keys.Any(segLine => Singleton<TransportManager>.instance.m_lines.m_buffer[segLine].Info.m_transportType == (TransportInfo.TransportType)Enum.Parse(typeof(TransportInfo.TransportType), sharedStopTypes.ToString())
                       && (sharedStopSegment.m_lines[segLine] & direction) == direction))
                    {
                        Log.Debug($"Remove sharedstoptype {sharedStopTypes}");
                        if (direction == NetInfo.Direction.Forward) sharedStopSegment.m_sharedStopTypesForward &= ~sharedStopTypes;
                        else if (direction == NetInfo.Direction.Backward) sharedStopSegment.m_sharedStopTypesBackward &= ~sharedStopTypes;
                    }
                }
                if (sharedStopSegment.m_lines.Count == 0)
                {
                    Singleton<NetManager>.instance.m_segments.m_buffer[sharedStopSegment.m_segment].m_flags &= ~NetSegment.Flags.StopAll;
                    sharedStopSegments.Remove(sharedStopSegment);
                    Log.Debug($"removed sharedsegment {segment}");
                    //sharedStopSegment.InitProps();
                    return true;
                }
                //sharedStopSegment.UpdateProps(direction);
                var flags = Singleton<NetManager>.instance.m_segments.m_buffer[sharedStopSegment.m_segment].m_flags;
                var inverted = (flags & NetSegment.Flags.Invert) == NetSegment.Flags.Invert;
                sharedStopSegment.UpdateStopFlags(inverted, out NetSegment.Flags stopflags);
                flags &= ~(NetSegment.Flags.StopRight | NetSegment.Flags.StopLeft | NetSegment.Flags.StopRight2 | NetSegment.Flags.StopLeft2);
                Singleton<NetManager>.instance.m_segments.m_buffer[sharedStopSegment.m_segment].m_flags = stopflags | flags;
                return true;
            }
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
