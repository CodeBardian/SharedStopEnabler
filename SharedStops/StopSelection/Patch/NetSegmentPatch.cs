using ColossalFramework;
using HarmonyLib;
using System;
using UnityEngine;

namespace SharedStopEnabler.StopSelection.Patch
{
    [HarmonyPatch(typeof(NetSegment))]
    [HarmonyPatch("GetClosestLanePosition")]
    [HarmonyPatch(
        new Type[] { typeof(Vector3), typeof(NetInfo.LaneType), typeof(VehicleInfo.VehicleType), typeof(VehicleInfo.VehicleCategory), typeof(VehicleInfo.VehicleType),
            typeof(bool), typeof(Vector3), typeof(int), typeof(float), typeof(Vector3), typeof(int), typeof(float)},
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
            ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out})]
    class NetSegmentPatch_GetClosestLanePosition
    {
        static bool Prefix(ref bool __result, NetSegment __instance, ref bool requireConnect)
        {
            if (requireConnect && __instance.Info.m_netAI is RoadBridgeAI)
            {
                requireConnect = false;
            }
            return true;
        }
    }
}
