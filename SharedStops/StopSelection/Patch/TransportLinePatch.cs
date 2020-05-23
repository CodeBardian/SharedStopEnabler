using ColossalFramework;
using ColossalFramework.PlatformServices;
using HarmonyLib;
using SharedStopEnabler.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using UnityEngine;

namespace SharedStopEnabler.StopSelection.Patch
{
    [HarmonyPatch(typeof(TransportTool), "RemoveStop")]
    class TransportLinePatch1
    {
        static void Postfix(ref IEnumerator __result, TransportTool __instance, TransportInfo ___m_prefab, Ray ___m_mouseRay, float ___m_mouseRayLength, ushort ___m_lastEditLine, Vector3 ___m_hitPosition)
        {
            try
            {
                ToolBase.RaycastOutput raycastOutput = RayCastWrapper.RayCast(__instance, ___m_prefab, ___m_mouseRay, ___m_mouseRayLength);              
                if (raycastOutput.m_netSegment != 0 && ___m_lastEditLine != 0)
                {
                    if (Singleton<NetManager>.instance.m_segments.m_buffer[(int)raycastOutput.m_netSegment].GetClosestLanePosition(___m_hitPosition, NetInfo.LaneType.Vehicle, ___m_prefab.m_vehicleType, out _, out uint laneID, out int laneindex, out _))
                    {
                        NetInfo.Direction direction = Singleton<NetManager>.instance.m_segments.m_buffer[(int)raycastOutput.m_netSegment].Info.m_lanes[laneindex].m_direction;
                        Singleton<SharedStopsTool>.instance.RemoveSharedStop(raycastOutput.m_netSegment, (SharedStopSegment.SharedStopTypes)Enum.Parse(typeof(SharedStopSegment.SharedStopTypes), ___m_prefab.m_transportType.ToString()), ___m_lastEditLine, direction);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"SSE: Exeption on removing stop {e}");
            }
        }
    }

    [HarmonyPatch(typeof(TransportTool), "CancelPrevStop")]
    class TransportLinePatch3
    {
        static void Postfix(ref IEnumerator __result, TransportTool __instance, TransportInfo ___m_prefab, Ray ___m_mouseRay, float ___m_mouseRayLength, ushort ___m_lastEditLine, Vector3 ___m_hitPosition)
        {
            try
            {
                ToolBase.RaycastOutput raycastOutput = RayCastWrapper.RayCast(__instance, ___m_prefab, ___m_mouseRay, ___m_mouseRayLength);
                if (raycastOutput.m_netSegment != 0 && ___m_lastEditLine != 0)
                {
                    if (Singleton<NetManager>.instance.m_segments.m_buffer[(int)raycastOutput.m_netSegment].GetClosestLanePosition(___m_hitPosition, NetInfo.LaneType.Vehicle, ___m_prefab.m_vehicleType, out _, out _, out int laneindex, out _))
                    {
                        NetInfo.Direction direction = Singleton<NetManager>.instance.m_segments.m_buffer[(int)raycastOutput.m_netSegment].Info.m_lanes[laneindex].m_direction;
                        Singleton<SharedStopsTool>.instance.RemoveSharedStop(raycastOutput.m_netSegment, (SharedStopSegment.SharedStopTypes)Enum.Parse(typeof(SharedStopSegment.SharedStopTypes), ___m_prefab.m_transportType.ToString()), ___m_lastEditLine, direction);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"SSE: Exeption on canceling stop {e}");
            }
        }
    }

    [HarmonyPatch(typeof(TransportTool), "MoveStop")]
    class TransportLinePatch4
    {
        static void Postfix(ref IEnumerator<bool> __result, TransportTool __instance, TransportInfo ___m_prefab, Ray ___m_mouseRay, float ___m_mouseRayLength, ushort ___m_lastEditLine, Vector3 ___m_hitPosition)
        {
            try
            {
                Log.Debug($"SSE:  moved stop {__result.Current}");
                //if (__result.Current)
                //{
                    ToolBase.RaycastOutput raycastOutput = RayCastWrapper.RayCast(__instance, ___m_prefab, ___m_mouseRay, ___m_mouseRayLength);
                    if (raycastOutput.m_netSegment != 0 && ___m_lastEditLine != 0)
                    {
                        if (Singleton<NetManager>.instance.m_segments.m_buffer[(int)raycastOutput.m_netSegment].GetClosestLanePosition(___m_hitPosition, NetInfo.LaneType.Vehicle, ___m_prefab.m_vehicleType, out _, out _, out int laneindex, out _))
                        {
                            NetInfo.Direction direction = Singleton<NetManager>.instance.m_segments.m_buffer[(int)raycastOutput.m_netSegment].Info.m_lanes[laneindex].m_direction;
                            Singleton<SharedStopsTool>.instance.AddSharedStop(raycastOutput.m_netSegment, (SharedStopSegment.SharedStopTypes)Enum.Parse(typeof(SharedStopSegment.SharedStopTypes), ___m_prefab.m_transportType.ToString()), ___m_lastEditLine, direction);
                        }
                    }
                //}
            }
            catch (Exception e)
            {
                Log.Error($"SSE: Exeption on moved stop {e}");
            }
        }
    }

    [HarmonyPatch(typeof(TransportTool), "AddStop")]
    class TransportLinePatch5
    {
        static void Postfix(ref IEnumerator __result, TransportTool __instance, TransportInfo ___m_prefab, Ray ___m_mouseRay, float ___m_mouseRayLength, ushort ___m_lastEditLine, Vector3 ___m_hitPosition)
        {
            try
            {
                var netManager = Singleton<NetManager>.instance;
                Log.Debug($"SSE: added stop");
                ToolBase.RaycastOutput raycastOutput = RayCastWrapper.RayCast(__instance, ___m_prefab, ___m_mouseRay, ___m_mouseRayLength);
                if (raycastOutput.m_netSegment != 0 && ___m_lastEditLine != 0)
                {
                    if (netManager.m_segments.m_buffer[(int)raycastOutput.m_netSegment].GetClosestLanePosition(___m_hitPosition, NetInfo.LaneType.Vehicle, ___m_prefab.m_vehicleType, out _, out uint laneID, out int laneindex, out _))
                    {
                        NetInfo.Direction direction = netManager.m_segments.m_buffer[(int)raycastOutput.m_netSegment].Info.m_lanes[laneindex].m_direction;
                        Singleton<SharedStopsTool>.instance.AddSharedStop(raycastOutput.m_netSegment, (SharedStopSegment.SharedStopTypes)Enum.Parse(typeof(SharedStopSegment.SharedStopTypes), ___m_prefab.m_transportType.ToString()), ___m_lastEditLine, direction);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"SSE: Exeption on adding stop {e}");
            }
        }
    }


    [HarmonyPatch(typeof(TransportTool), "GetStopPosition")]
    class TransportToolPatch2
    {
        static bool Prefix(ref bool __result, TransportInfo info, ushort segment, ushort building, ushort firstStop, ref Vector3 hitPos, out bool fixedPlatform)
        {
            __result = Singleton<SharedStopsTool>.instance.GetStopPosition(out bool skipOriginal, info, segment, building, firstStop, ref hitPos, out fixedPlatform);

            return !skipOriginal;
        }
    }
}
