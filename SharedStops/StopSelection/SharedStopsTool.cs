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

        public Vector3 m_lastEditPoint;

        private bool additionalStopsSet = false;
        private bool additionalStopsRemoved = true;

        private int m_building => (int)typeof(TransportTool).GetField("m_building", BindingFlags.Public | BindingFlags.Instance).GetValue(Singleton<TransportTool>.instance);

        public void Start()
        {
            try
            {
                StopsUtil.EnableElevatedStops();
                sharedStopSegments = new List<SharedStopSegment>();

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

        private void RecalculateSharedStopSegments()
        {
            foreach (TransportLine line in Singleton<TransportManager>.instance.m_lines.m_buffer)
            {
                if (line.Info.m_class.m_subService != ItemClass.SubService.PublicTransportBus && line.Info.m_class.m_subService != ItemClass.SubService.PublicTransportTram &&
                    line.Info.m_class.m_subService != ItemClass.SubService.PublicTransportTrolleybus && line.Info.m_class.name != "Sightseeing Bus Line") continue;

                ushort lineID = Singleton<NetManager>.instance.m_nodes.m_buffer[line.m_stops].m_transportLine;
                Log.Debug($"Checking line {lineID}, {line.Info.m_class.m_subService}, {line.m_stops}");

                if (lineID == 0) continue;
                ushort stops = line.m_stops;
                if (stops == 0) continue;
                for (;;)
                {
                    stops = TransportLine.GetNextStop(stops);
                    uint lane = Singleton<NetManager>.instance.m_nodes.m_buffer[stops].m_lane;
                    Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[stops].m_position;
                    ushort segment = Singleton<NetManager>.instance.m_lanes.m_buffer[lane].m_segment;
                    Log.Debug($"stop {stops} on lane {lane} on segment {segment}");
                    if (Singleton<NetManager>.instance.m_segments.m_buffer[segment].GetClosestLanePosition(position, NetInfo.LaneType.Vehicle, line.Info.m_vehicleType, out _, out _, out int laneindex, out _))
                    {
                        NetInfo.Direction direction = Singleton<NetManager>.instance.m_segments.m_buffer[segment].Info.m_lanes[laneindex].m_direction;
                        Singleton<SharedStopsTool>.instance.AddSharedStop(segment, (SharedStopSegment.SharedStopTypes)Enum.Parse(typeof(SharedStopSegment.SharedStopTypes), line.Info.m_transportType.ToString()), lineID, direction);
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
                var sharedStopSegment = sharedStopSegments[sharedStopSegments.FindIndex(s => s.m_segment == segment)];
                if (direction == NetInfo.Direction.Forward) sharedStopSegment.m_sharedStopTypesForward &= ~sharedStopTypes;
                else if (direction == NetInfo.Direction.Backward) sharedStopSegment.m_sharedStopTypesBackward &= ~sharedStopTypes;
                if (sharedStopSegment.m_lines.Keys.Contains(line)) //TODO: remove unnecessary checks
                {
                    sharedStopSegment.m_lines[line] &= ~direction;
                    if (sharedStopSegment.m_lines[line] == NetInfo.Direction.None)
                    {
                        sharedStopSegment.m_lines.Remove(line);
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

        public void GetStopPosition(ref bool result, TransportInfo info, ushort segment, ushort building, ushort firstStop, ref Vector3 hitPos, out bool fixedPlatform)
        {
            NetManager netManager = Singleton<NetManager>.instance;
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;

            fixedPlatform = false;

            if ((int)segment != 0) //hover segment
            {
                if ((netManager.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None)
                {
                    building = NetSegment.FindOwnerBuilding(segment, 363f);
                    if ((int)building != 0)  //found nearby building
                    {
                        BuildingInfo info1 = buildingManager.m_buildings.m_buffer[(int)building].Info;
                        TransportInfo transportLineInfo1 = info1.m_buildingAI.GetTransportLineInfo();
                        TransportInfo transportLineInfo2 = info1.m_buildingAI.GetSecondaryTransportLineInfo();
                        if (transportLineInfo1 != null && transportLineInfo1.m_transportType == info.m_transportType ||  transportLineInfo2 != null && transportLineInfo2.m_transportType == info.m_transportType)
                            segment = (ushort)0; //metro stop results in segment = 0
                        else
                            building = (ushort)0;
                    }
                }
           
                if ((int)segment != 0 && netManager.m_segments.m_buffer[(int)segment].GetClosestLanePosition(hitPos, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, info.m_vehicleType, out Vector3 closestPedestrianLane, out uint laneid1, out int laneIndex1, out _))
                {
                    if (netManager.m_segments.m_buffer[(int)segment].GetClosestLanePosition(closestPedestrianLane, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, info.m_vehicleType, out Vector3 position2, out uint laneID2, out int laneIndex2, out float laneOffset2))
                    {
                        NetLane.Flags flags3 = (NetLane.Flags)netManager.m_lanes.m_buffer[(int)segment].m_flags;
                        flags3 &= NetLane.Flags.Stops;
                        if (flags3 != NetLane.Flags.None && info.m_stopFlag != NetLane.Flags.None && flags3 != info.m_stopFlag)
                        {
                            Log.Debug("Flags set");
                            result = false;
                            return;
                            //return false;
                        }
                        float stopOffset = netManager.m_segments.m_buffer[(int)segment].Info.m_lanes[laneIndex2].m_stopOffset;
                        if ((netManager.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)  //check for inverted lanes
                            stopOffset = -stopOffset;
                        Log.Debug($"hitpos:{hitPos} pedestrianlane: {laneIndex1}, vehiclelane: {laneIndex2}");
                        netManager.m_lanes.m_buffer[laneID2].CalculateStopPositionAndDirection(0.5019608f, stopOffset, out hitPos, out Vector3 direction);
                        fixedPlatform = true;
                        result = true;
                    }
                }
            }
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

        public int GetLineCount(Vector3 stopPosition, Vector3 stopDirection, TransportInfo.TransportType transportType)
        {
            NetManager instance = Singleton<NetManager>.instance;
            TransportManager instance2 = Singleton<TransportManager>.instance;
            stopDirection.Normalize();
            Segment3 segment = new Segment3(stopPosition - stopDirection * 16f, stopPosition + stopDirection * 16f);
            Vector3 vector = segment.Min();
            Vector3 vector2 = segment.Max();
            int num = Mathf.Max((int)((vector.x - 4f) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((vector.z - 4f) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((vector2.x + 4f) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((vector2.z + 4f) / 64f + 135f), 269);
            int num5 = 0;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num6 = instance.m_nodeGrid[i * 270 + j];
                    int num7 = 0;
                    while (num6 != 0)
                    {
                        ushort transportLine = instance.m_nodes.m_buffer[(int)num6].m_transportLine;
                        if (transportLine != 0)
                        {
                            TransportInfo info = instance2.m_lines.m_buffer[(int)transportLine].Info;
                            if (info.m_transportType == transportType && (instance2.m_lines.m_buffer[(int)transportLine].m_flags & TransportLine.Flags.Temporary) == TransportLine.Flags.None && segment.DistanceSqr(instance.m_nodes.m_buffer[(int)num6].m_position) < 16f)
                            {
                                num5++;
                            }
                        }
                        num6 = instance.m_nodes.m_buffer[(int)num6].m_nextGridNode;
                        if (++num7 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return num5;
        }
    }
}
