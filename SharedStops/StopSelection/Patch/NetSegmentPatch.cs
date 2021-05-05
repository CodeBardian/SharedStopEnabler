using ColossalFramework;
using HarmonyLib;
using System;
using UnityEngine;

namespace SharedStopEnabler.StopSelection.Patch
{
    [HarmonyPatch(typeof(NetSegment))]
    [HarmonyPatch("GetClosestLanePosition")]
    [HarmonyPatch(
        new Type[] { typeof(Vector3), typeof(NetInfo.LaneType), typeof(VehicleInfo.VehicleType), typeof(VehicleInfo.VehicleType),
            typeof(bool), typeof(Vector3), typeof(int), typeof(float), typeof(Vector3), typeof(int), typeof(float)},
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
            ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out})]
    class NetSegmentPatch_GetClosestLanePosition
    {
        static bool Prefix(ref bool __result, NetSegment __instance, Vector3 point, NetInfo.LaneType laneTypes, VehicleInfo.VehicleType vehicleTypes, ref bool requireConnect)
        {
            if (requireConnect && vehicleTypes.IsSharedStopTransport() && __instance.Info.m_netAI is RoadBridgeAI)
            {
                requireConnect = false;
            }
            return true;
        }
    }
}
