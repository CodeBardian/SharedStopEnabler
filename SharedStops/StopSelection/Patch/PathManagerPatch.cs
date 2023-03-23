using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using SharedStopEnabler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SharedStopEnabler.StopSelection.Patch
{
    [HarmonyPatch(typeof(PathManager))]
    [HarmonyPatch("FindPathPosition")]
    [HarmonyPatch(
        new Type[] { typeof(Vector3), typeof(ItemClass.Service), typeof(ItemClass.Service), typeof(NetInfo.LaneType), typeof(VehicleInfo.VehicleType), typeof(VehicleInfo.VehicleCategory),
            typeof(VehicleInfo.VehicleType), typeof(bool), typeof(bool), typeof(float), typeof(bool), typeof(PathUnit.Position), typeof(PathUnit.Position), typeof(float), typeof(float)},
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
            ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out})]
    class PathManagerPatch_FindPathPosition
    {
        static void Postfix(Vector3 position, ref PathUnit.Position pathPosA, VehicleInfo.VehicleType stopType, VehicleInfo.VehicleCategory vehicleCategory)
        {
            NetSegment segment = Singleton<NetManager>.instance.m_segments.m_buffer[pathPosA.m_segment];
            int stop = pathPosA.m_lane;

            if (stopType == VehicleInfo.VehicleType.Car && stop > 1)
            {  
                segment.GetClosestLanePosition(position, NetInfo.LaneType.Vehicle, stopType, vehicleCategory, out Vector3 pos, out uint laneID, out int laneindex, out float laneOffset);

                NetInfo.Direction dir = segment.Info.m_lanes[laneindex].m_direction;

                int stopLane = Array.FindIndex(segment.Info.m_sortedLanes, s => s == stop);
                int drivingLane = Array.FindIndex(segment.Info.m_sortedLanes, s => s == laneindex);

                if (dir == NetInfo.Direction.Backward && stopLane > drivingLane)
                {
                    pathPosA.m_lane = (byte)segment.Info.m_sortedLanes[drivingLane - 1];
                }
                else if (dir == NetInfo.Direction.Forward && stopLane < drivingLane)
                    pathPosA.m_lane = (byte)segment.Info.m_sortedLanes[drivingLane + 1];
            }
        }
    }
}
