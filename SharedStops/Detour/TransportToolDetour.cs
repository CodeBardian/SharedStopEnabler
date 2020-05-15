using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using ColossalFramework;
using ColossalFramework.Math;
using SharedStopEnabler.RedirectionFramework.Attributes;
using UnityEngine;
using SharedStopEnabler.StopSelection;
using SharedStopEnabler.Util;

namespace SharedStopEnabler.Detour
{
    [TargetType(typeof(TransportTool))]
    public class TransportToolDetour : TransportTool
    {
        private bool additionalStopsSet = false;
        private bool additionalStopsRemoved = true;

        [RedirectMethod]
        private bool GetStopPosition(TransportInfo info, ushort segment, ushort building, ushort firstStop, ref Vector3 hitPos, out bool fixedPlatform)
        {
            bool alternateMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            NetManager netManager = Singleton<NetManager>.instance;
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            TransportManager transportManager = Singleton<TransportManager>.instance;

            fixedPlatform = false;

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
       
                if ((int)segment != 0 && alternateMode && !additionalStopsSet)  //set additional stoptypes
                {
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
                    additionalStopsRemoved = true;
                    additionalStopsSet = false;
                    if (netManager.m_segments.m_buffer[(int)segment].HasStops(segment)) goto Main;
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

                Main:
                if ((int)segment != 0 && netManager.m_segments.m_buffer[(int)segment].GetClosestLanePosition(hitPos, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, info.m_vehicleType, out Vector3 closestPedestrianLane, out uint laneid1, out _, out _))
                {
                    if (netManager.m_segments.m_buffer[(int)segment].GetClosestLanePosition(closestPedestrianLane, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, info.m_vehicleType, out Vector3 position2, out uint laneID2, out int laneIndex2, out float laneOffset2))
                    {
                        NetLane.Flags flags3 = (NetLane.Flags)netManager.m_lanes.m_buffer[(int)segment].m_flags;
                        flags3 &= NetLane.Flags.Stops;
                        if (flags3 != NetLane.Flags.None && info.m_stopFlag != NetLane.Flags.None && flags3 != info.m_stopFlag)
                        {
                            return false;
                        }
                        float stopOffset = netManager.m_segments.m_buffer[(int)segment].Info.m_lanes[laneIndex2].m_stopOffset;
                        if ((netManager.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)  //check for inverted lanes
                            stopOffset = -stopOffset;
                        netManager.m_lanes.m_buffer[laneID2].CalculateStopPositionAndDirection(0.5019608f, stopOffset, out hitPos, out Vector3 direction);
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

        [RedirectReverse]
        private int GetLineCount(Vector3 stopPosition, Vector3 stopDirection, TransportInfo.TransportType transportType)
        {
            Debug.Log("GetLineCount");
            return 0;
        }

        private ushort m_line => (ushort)typeof(TransportTool).GetField("m_line", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
    }
}
