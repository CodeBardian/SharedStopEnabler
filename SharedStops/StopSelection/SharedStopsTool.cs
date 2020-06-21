using ColossalFramework;
using ColossalFramework.Math;
using SharedStopEnabler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SharedStopEnabler.StopSelection
{
    class SharedStopsTool : MonoBehaviour
    {

        public List<SharedStopSegment> sharedStopSegments;

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

                Log.Info($"successful startup");
            }
            catch (Exception e)
            {
                Log.Error($"Failed on startup {e}");
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
                    Log.Debug($"add to existing line {segment}, {sharedStopTypes}, {line}, {direction}");
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

        public bool GetStopPosition(out bool skipOriginal, TransportInfo info, ushort segment, ushort building, ushort firstStop, ref Vector3 hitPos, out bool fixedPlatform)
        {
            bool alternateMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            NetManager netManager = Singleton<NetManager>.instance;
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;

            skipOriginal = true;
            fixedPlatform = false;

            if (info.m_transportType == TransportInfo.TransportType.Pedestrian || (alternateMode && segment == 0 ))
            {
                skipOriginal = false;
                return false;
            }

            if ((int)segment != 0 && alternateMode && !additionalStopsSet)  //set additional stoptypes
            {
                //Log.Debug("called set stoptypes");
                additionalStopsSet = true;
                additionalStopsRemoved = false;
                for (int i = 1; i < netManager.m_segments.m_buffer[(int)segment].Info.m_lanes.Length - 2; i++)
                {
                    uint index = (uint)netManager.m_segments.m_buffer[(int)segment].Info.m_sortedLanes[i];
                    if (IsValidLane(segment, (uint)i, info))
                    {
                        Log.Debug($"SharedStops calculate new stoptypes: {segment}, Stopflag: {info.m_stopFlag}, hitpos. {hitPos.x} {hitPos.y} {hitPos.z}, vehicleType: {info.m_vehicleType}");
                        netManager.m_segments.m_buffer[(int)segment].Info.m_lanes[index].m_stopType |= info.m_vehicleType;                      
                    }
                }
            }

            if ((int)segment != 0 && !alternateMode && !additionalStopsRemoved)  //remove additional stoptypes
            {
                //Log.Debug("called remove stoptypes");
                additionalStopsRemoved = true;
                additionalStopsSet = false;
                if (netManager.m_segments.m_buffer[(int)segment].HasStops(segment))
                {
                    skipOriginal = false;
                    return false;
                }
                for (int i = 1; i < netManager.m_segments.m_buffer[(int)segment].Info.m_lanes.Length - 2; i++)
                {
                    uint index = (uint)netManager.m_segments.m_buffer[(int)segment].Info.m_sortedLanes[i];
                    if ((netManager.m_segments.m_buffer[(int)segment].Info.m_lanes[index].m_stopType & info.m_vehicleType) == info.m_vehicleType)
                    {
                        Log.Debug($"SharedStops remove new stoptypes: {segment}, Stopflag: {info.m_stopFlag}, hitpos. {hitPos.x} {hitPos.y} {hitPos.z}, vehicleType: {info.m_vehicleType}");
                        netManager.m_segments.m_buffer[(int)segment].Info.m_lanes[index].m_stopType &= ~info.m_vehicleType;
                    }
                }
            }

            if ((int)segment != 0) //hover segment
            {
                if ((netManager.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None)  //rewrite flagset extension method
                {
                    building = NetSegment.FindOwnerBuilding(segment, 363f);
                    if ((int)building != 0)  //found nearby building
                    {
                        BuildingInfo info1 = buildingManager.m_buildings.m_buffer[(int)building].Info;
                        TransportInfo transportLineInfo1 = info1.m_buildingAI.GetTransportLineInfo();
                        TransportInfo transportLineInfo2 = info1.m_buildingAI.GetSecondaryTransportLineInfo();
                        if (!alternateMode && transportLineInfo1 != null && transportLineInfo1.m_transportType == info.m_transportType || !alternateMode && transportLineInfo2 != null && transportLineInfo2.m_transportType == info.m_transportType)
                            segment = (ushort)0; //metro stop results in segment = 0
                        else
                            building = (ushort)0;
                    }
                }
           
                if ((int)segment != 0 && netManager.m_segments.m_buffer[(int)segment].GetClosestLanePosition(hitPos, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, info.m_vehicleType, out Vector3 closestPedestrianLane, out uint laneid1, out _, out _))
                {
                    if (netManager.m_segments.m_buffer[(int)segment].GetClosestLanePosition(closestPedestrianLane, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, info.m_vehicleType, out Vector3 position2, out uint laneID2, out int laneIndex2, out float laneOffset2))
                    {
                        NetLane.Flags flags3 = (NetLane.Flags)netManager.m_lanes.m_buffer[(int)segment].m_flags;
                        flags3 &= NetLane.Flags.Stops;
                        if (flags3 != NetLane.Flags.None && info.m_stopFlag != NetLane.Flags.None && flags3 != info.m_stopFlag)
                        {
                            Log.Debug("Flags set");
                            return false;
                        }
                        float stopOffset = netManager.m_segments.m_buffer[(int)segment].Info.m_lanes[laneIndex2].m_stopOffset;
                        if ((netManager.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)  //check for inverted lanes
                            stopOffset = -stopOffset;
                        netManager.m_lanes.m_buffer[laneID2].CalculateStopPositionAndDirection(0.5019608f, stopOffset, out hitPos, out Vector3 direction);
                        //Log.Debug($"pedesrianlane: {laneid1}, vehiclelane: {laneID2}, stoppos: {hitPos}");
                        fixedPlatform = true;
                        return true;
                    }
                }
            }

            if (!alternateMode && (int)building != 0)
            {
                ushort parentBuilding = 0;
                if ((buildingManager.m_buildings.m_buffer[(int)building].m_flags & Building.Flags.Untouchable) != Building.Flags.None)
                {
                    parentBuilding = Building.FindParentBuilding(building);
                }
                if (this.m_building != 0 && (int)firstStop != 0 && (this.m_building == (int)building || this.m_building == (int)parentBuilding))  //seems to never be called?
                {
                    hitPos = netManager.m_nodes.m_buffer[(int)firstStop].m_position;
                    return true;
                }
                VehicleInfo randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, info.m_class.m_service, info.m_class.m_subService, info.m_class.m_level);
                if (randomVehicleInfo != null)
                {
                    BuildingInfo buildingInfo = buildingManager.m_buildings.m_buffer[(int)building].Info;
                    TransportInfo transportLineInfo1 = buildingInfo.m_buildingAI.GetTransportLineInfo();
                    if (transportLineInfo1 == null && (int)parentBuilding != 0)
                    {
                        building = parentBuilding;
                        buildingInfo = buildingManager.m_buildings.m_buffer[(int)building].Info;
                        transportLineInfo1 = buildingInfo.m_buildingAI.GetTransportLineInfo();
                    }
                    TransportInfo transportLineInfo2 = buildingInfo.m_buildingAI.GetSecondaryTransportLineInfo();
                    if (transportLineInfo1 != null && transportLineInfo1.m_transportType == info.m_transportType || transportLineInfo2 != null && transportLineInfo2.m_transportType == info.m_transportType)
                    {
                        Vector3 vector3 = Vector3.zero;
                        int num2 = 1000000;
                        for (int index = 0; index < 12; ++index)
                        {
                            Randomizer randomizer = new Randomizer((ulong)index);
                            buildingInfo.m_buildingAI.CalculateSpawnPosition(building, ref buildingManager.m_buildings.m_buffer[(int)building], ref randomizer, randomVehicleInfo, out Vector3 spawnposition, out Vector3 target);
                            int linecount = 0;
                            if (info.m_avoidSameStopPlatform)
                                linecount = this.GetLineCount(spawnposition, target - spawnposition, info.m_transportType);  //
                            if (linecount < num2)
                            {
                                vector3 = spawnposition;
                                num2 = linecount;
                            }
                            else if (linecount == num2 && (double)Vector3.SqrMagnitude(spawnposition - hitPos) < (double)Vector3.SqrMagnitude(vector3 - hitPos))
                                vector3 = spawnposition;
                        }
                        if ((int)firstStop != 0)
                        {
                            Vector3 position = netManager.m_nodes.m_buffer[(int)firstStop].m_position;
                            if ((double)Vector3.SqrMagnitude(position - vector3) < 16384.0)
                            {
                                uint lane = netManager.m_nodes.m_buffer[(int)firstStop].m_lane;
                                if ((int)lane != 0)
                                {
                                    ushort segment1 = netManager.m_lanes.m_buffer[lane].m_segment;
                                    if ((int)segment1 != 0 && (netManager.m_segments.m_buffer[(int)segment1].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None)
                                    {
                                        ushort ownerBuilding = NetSegment.FindOwnerBuilding(segment1, 363f);
                                        if ((int)building == (int)ownerBuilding)
                                        {
                                            hitPos = position;
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        hitPos = vector3;
                        return num2 != 1000000;
                    }
                }
                return false;
            }
            return false;
        }

        private bool IsValidLane(uint segment, uint laneIndex, TransportInfo info) //check left and right lanes
        {
            var netManager = Singleton<NetManager>.instance;
            uint index = (uint)netManager.m_segments.m_buffer[(int)segment].Info.m_sortedLanes[laneIndex];
            if ((netManager.m_segments.m_buffer[(int)segment].Info.m_lanes[index].m_vehicleType == VehicleInfo.VehicleType.None) && ((netManager.m_segments.m_buffer[(int)segment].Info.m_lanes[index].m_stopType & info.m_vehicleType) != info.m_vehicleType))
            {
                if (info.m_transportType == TransportInfo.TransportType.Bus)
                {
                    int index1 = netManager.m_segments.m_buffer[(int)segment].Info.m_sortedLanes[laneIndex + 1];
                    int index2 = netManager.m_segments.m_buffer[(int)segment].Info.m_sortedLanes[laneIndex - 1];
                    if (((netManager.m_segments.m_buffer[(int)segment].Info.m_lanes[index1].m_vehicleType & info.m_vehicleType) != info.m_vehicleType) || ((netManager.m_segments.m_buffer[(int)segment].Info.m_lanes[index2].m_vehicleType & info.m_vehicleType) != info.m_vehicleType))
                    {
                        return false;
                    }
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
