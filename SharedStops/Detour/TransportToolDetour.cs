using System;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using AdvancedStopSelection.RedirectionFramework.Attributes;
using UnityEngine;

namespace AdvancedStopSelection.Detour
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

            fixedPlatform = false;

            if ((int)segment != 0) //hover segment
            {
                if ((netManager.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None)  //rewrite hasflags extension method
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
                Vector3 closestPedestrianLane;
                uint laneID1;
                int laneIndex1;
                float laneOffset1;
                                
                if ((int)segment != 0 && alternateMode && !additionalStopsSet)  //set additional stoptypes
                {
                    for (int i = 1; i < netManager.m_segments.m_buffer[(int)segment].Info.m_lanes.Length - 2; i++)
                    {
                        uint index = (uint)netManager.m_segments.m_buffer[(int)segment].Info.m_sortedLanes[i];
                        if ((netManager.m_segments.m_buffer[(int)segment].Info.m_lanes[index].m_vehicleType == VehicleInfo.VehicleType.None) && (netManager.m_segments.m_buffer[(int)segment].Info.m_lanes[index].m_stopType != info.m_vehicleType))  // TODO: change to not FlagSet
                        {
                            Debug.Log($"SharedStops calculate new stoptypes: {segment}, Stopflag: {info.m_stopFlag}, hitpos. {hitPos.x} {hitPos.y} {hitPos.z}, vehicleType: {info.m_vehicleType}");
                            netManager.m_segments.m_buffer[(int)segment].Info.m_lanes[index].m_stopType |= info.m_vehicleType;
                            additionalStopsSet = true;
                            additionalStopsRemoved = false;
                        }
                    }
                }
                if ((int)segment != 0 && !alternateMode && !additionalStopsRemoved)  //remove additional stoptypes
                {
                    additionalStopsRemoved = true;
                    additionalStopsSet = false;
                    for (int i = 1; i < netManager.m_segments.m_buffer[(int)segment].Info.m_lanes.Length - 2; i++)
                    {
                        uint index = (uint)netManager.m_segments.m_buffer[(int)segment].Info.m_sortedLanes[i];
                        netManager.m_segments.m_buffer[(int)segment].GetClosestLanePosition(hitPos, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, info.m_vehicleType, out closestPedestrianLane, out laneID1, out laneIndex1, out laneOffset1);
                        if (((NetLane.Flags)netManager.m_lanes.m_buffer[laneID1].m_flags & NetLane.Flags.Stop) != NetLane.Flags.None) continue;
                        if ((netManager.m_segments.m_buffer[(int)segment].Info.m_lanes[index].m_stopType & info.m_vehicleType) == info.m_vehicleType)
                        {
                            Debug.Log($"SharedStops remove new stoptypes: {segment}, Stopflag: {info.m_stopFlag}, hitpos. {hitPos.x} {hitPos.y} {hitPos.z}, vehicleType: {info.m_vehicleType}");
                            netManager.m_segments.m_buffer[(int)segment].Info.m_lanes[index].m_stopType &= ~info.m_vehicleType;
                        }
                    }
                }

                if ((int)segment != 0 && netManager.m_segments.m_buffer[(int)segment].GetClosestLanePosition(hitPos, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, info.m_vehicleType, out closestPedestrianLane, out laneID1, out laneIndex1, out laneOffset1))
                {
                    //if (info.m_vehicleType == VehicleInfo.VehicleType.None) //when does this happen? 
                    //{
                    //    Debug.LogWarning("VEHICLETYPE not defined");
                    //    NetLane.Flags flags1 = (NetLane.Flags)((int)netManager.m_lanes.m_buffer[laneID1].m_flags & 768); //results in none, stop, stop2, or stops
                    //    NetLane.Flags flags2 = info.m_stopFlag;
                    //    NetInfo info1 = netManager.m_segments.m_buffer[(int)segment].Info;
                    //    if (info1.m_vehicleTypes != VehicleInfo.VehicleType.None)
                    //        flags2 = NetLane.Flags.None;
                    //    if (flags1 != NetLane.Flags.None && flags2 != NetLane.Flags.None && flags1 != flags2)
                    //        return false;
                    //    float stopOffset = info1.m_lanes[laneIndex1].m_stopOffset;
                    //    if ((netManager.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)
                    //        stopOffset = -stopOffset;
                    //    Vector3 direction;
                    //    netManager.m_lanes.m_buffer[laneID1].CalculateStopPositionAndDirection(0.5019608f, stopOffset, out hitPos, out direction);
                    //    fixedPlatform = true;
                    //    return true;
                    //}
                    Vector3 position2;
                    uint laneID2;
                    int laneIndex2;
                    float laneOffset2;
                    if (netManager.m_segments.m_buffer[(int)segment].GetClosestLanePosition(closestPedestrianLane, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, info.m_vehicleType, out position2, out laneID2, out laneIndex2, out laneOffset2))
                    {
                        //Debug.Log($"SharedStops hover Segment: {segment}, Building: {building}, NetFlags: {instance1.m_lanes.m_buffer[laneID1].m_flags}, vehicleType: {info.m_vehicleType}");
                        //NetLane.Flags flags = (NetLane.Flags)((int)instance1.m_lanes.m_buffer[laneID1].m_flags & 768);
                        //if (flags != NetLane.Flags.None && info.m_stopFlag != NetLane.Flags.None && flags != info.m_stopFlag)
                        //    return true;
                        float stopOffset = netManager.m_segments.m_buffer[(int)segment].Info.m_lanes[laneIndex2].m_stopOffset;
                        if ((netManager.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)  //check for inverted lanes
                            stopOffset = -stopOffset;
                        Vector3 direction;
                        netManager.m_lanes.m_buffer[laneID2].CalculateStopPositionAndDirection(0.5019608f, stopOffset, out hitPos, out direction);
                        fixedPlatform = true; //true or false_? true better for now
                        return true;
                    }
                }
            }

            if (!alternateMode && (int)building != 0)
            {
                ushort num1 = 0;
                if ((buildingManager.m_buildings.m_buffer[(int)building].m_flags & Building.Flags.Untouchable) != Building.Flags.None)
                    num1 = Building.FindParentBuilding(building);
                if (this.m_building != 0 && (int)firstStop != 0 && (this.m_building == (int)building || this.m_building == (int)num1))
                {
                    hitPos = netManager.m_nodes.m_buffer[(int)firstStop].m_position;
                    return true;
                }
                VehicleInfo randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, info.m_class.m_service, info.m_class.m_subService, info.m_class.m_level);
                if (randomVehicleInfo != null)
                {
                    BuildingInfo info1 = buildingManager.m_buildings.m_buffer[(int)building].Info;
                    TransportInfo transportLineInfo1 = info1.m_buildingAI.GetTransportLineInfo();
                    if (transportLineInfo1 == null && (int)num1 != 0)
                    {
                        building = num1;
                        info1 = buildingManager.m_buildings.m_buffer[(int)building].Info;
                        transportLineInfo1 = info1.m_buildingAI.GetTransportLineInfo();
                    }
                    TransportInfo transportLineInfo2 = info1.m_buildingAI.GetSecondaryTransportLineInfo();
                    if (transportLineInfo1 != null && transportLineInfo1.m_transportType == info.m_transportType || transportLineInfo2 != null && transportLineInfo2.m_transportType == info.m_transportType)
                    {
                        Vector3 vector3 = Vector3.zero;
                        int num2 = 1000000;
                        for (int index = 0; index < 12; ++index)
                        {
                            Randomizer randomizer = new Randomizer((ulong)index);
                            Vector3 position;
                            Vector3 target;
                            info1.m_buildingAI.CalculateSpawnPosition(building, ref buildingManager.m_buildings.m_buffer[(int)building], ref randomizer, randomVehicleInfo, out position, out target);
                            int num3 = 0;
                            if (info.m_avoidSameStopPlatform)
                                num3 = this.GetLineCount(position, target - position, info.m_transportType);  //
                            if (num3 < num2)
                            {
                                vector3 = position;
                                num2 = num3;
                            }
                            else if (num3 == num2 && (double)Vector3.SqrMagnitude(position - hitPos) < (double)Vector3.SqrMagnitude(vector3 - hitPos))
                                vector3 = position;
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

        [RedirectReverse]
        private int GetLineCount(Vector3 stopPosition, Vector3 stopDirection, TransportInfo.TransportType transportType)
        {
            Debug.Log("GetLineCount");
            return 0;
        }

        private ushort m_line => (ushort)typeof(TransportTool).GetField("m_line",
            BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
    }
}
