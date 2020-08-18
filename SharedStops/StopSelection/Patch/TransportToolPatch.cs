using ColossalFramework;
using ColossalFramework.PlatformServices;
using HarmonyLib;
using SharedStopEnabler.Util;
using SharedStopEnabler.StopSelection;
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
    class TransportToolPatch_RemoveStop
    {
        static void Postfix(ref IEnumerator __result, TransportTool __instance, TransportInfo ___m_prefab, Ray ___m_mouseRay, float ___m_mouseRayLength, ushort ___m_lastEditLine, Vector3 ___m_hitPosition)
        {
            try
            {
                ToolBase.RaycastOutput raycastOutput = RayCastWrapper.RayCast(__instance, ___m_prefab, ___m_mouseRay, ___m_mouseRayLength);
                if (___m_prefab.m_transportType.IsSharedStopTransport() && raycastOutput.m_netSegment != 0 && ___m_lastEditLine != 0)
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
    class TransportToolPatch_CancelPrevStop
    {
        static void Postfix(ref IEnumerator __result, TransportTool __instance, TransportInfo ___m_prefab, Ray ___m_mouseRay, float ___m_mouseRayLength, ushort ___m_lastEditLine, Vector3 ___m_hitPosition)
        {
            try
            {
                ToolBase.RaycastOutput raycastOutput = RayCastWrapper.RayCast(__instance, ___m_prefab, ___m_mouseRay, ___m_mouseRayLength);
                if (___m_prefab.m_transportType.IsSharedStopTransport() && raycastOutput.m_netSegment != 0 && ___m_lastEditLine != 0)
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
    class TransportToolPatch_MoveStop
    {
        static void Postfix(ref IEnumerator<bool> __result, TransportTool __instance, TransportInfo ___m_prefab, Ray ___m_mouseRay, float ___m_mouseRayLength,
            ushort ___m_lastEditLine, Vector3 ___m_hitPosition, ushort ___m_line, int ___m_lastMoveIndex)
        {
            try
            {
                Log.Debug($"SSE:  moved stop {__result.Current}");
                ToolBase.RaycastOutput raycastOutput = RayCastWrapper.RayCast(__instance, ___m_prefab, ___m_mouseRay, ___m_mouseRayLength);
                Log.Debug($"{raycastOutput.m_hitPos.x} {raycastOutput.m_hitPos.y} {raycastOutput.m_hitPos.z} on {raycastOutput.m_netSegment}");
                if (___m_prefab.m_transportType.IsSharedStopTransport() && raycastOutput.m_netSegment != 0 && ___m_lastEditLine != 0)
                {
                    if (Singleton<NetManager>.instance.m_segments.m_buffer[(int)raycastOutput.m_netSegment].GetClosestLanePosition(___m_hitPosition, NetInfo.LaneType.Vehicle, ___m_prefab.m_vehicleType, out _, out _, out int laneindex, out _))
                    {
                        NetInfo.Direction direction = Singleton<NetManager>.instance.m_segments.m_buffer[(int)raycastOutput.m_netSegment].Info.m_lanes[laneindex].m_direction;
                        Singleton<SharedStopsTool>.instance.AddSharedStop(raycastOutput.m_netSegment, (SharedStopSegment.SharedStopTypes)Enum.Parse(typeof(SharedStopSegment.SharedStopTypes), ___m_prefab.m_transportType.ToString()), ___m_lastEditLine, direction);
                    }

                    ushort stops = Singleton<TransportManager>.instance.m_lines.m_buffer[___m_line].m_stops;
                    if (stops == 0) return;
                    for (int i = 0; i < ___m_lastMoveIndex; i++)
                    {
                        stops = TransportLine.GetNextStop(stops);
                        if (stops == Singleton<TransportManager>.instance.m_lines.m_buffer[___m_line].m_stops) break;
                    }
                    if (stops == 0) return;
                    uint lane = Singleton<NetManager>.instance.m_nodes.m_buffer[stops].m_lane;
                    Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[stops].m_position;
                    ushort segment = Singleton<NetManager>.instance.m_lanes.m_buffer[lane].m_segment;
                    Log.Debug($"old lane {lane} on segment {segment}");
                    if (Singleton<NetManager>.instance.m_segments.m_buffer[(int)raycastOutput.m_netSegment].GetClosestLanePosition(position, NetInfo.LaneType.Vehicle, ___m_prefab.m_vehicleType, out _, out _, out laneindex, out _))
                    {
                        NetInfo.Direction direction = Singleton<NetManager>.instance.m_segments.m_buffer[(int)raycastOutput.m_netSegment].Info.m_lanes[laneindex].m_direction;
                        Singleton<SharedStopsTool>.instance.RemoveSharedStop(raycastOutput.m_netSegment, (SharedStopSegment.SharedStopTypes)Enum.Parse(typeof(SharedStopSegment.SharedStopTypes), ___m_prefab.m_transportType.ToString()), ___m_lastEditLine, direction);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"SSE: Exeption on moved stop {e}");
            }
        }
    }

    [HarmonyPatch(typeof(TransportTool), "AddStop")]
    class TransportToolPatch_AddStop
    {
        static void Postfix(ref IEnumerator __result, TransportTool __instance, TransportInfo ___m_prefab, Ray ___m_mouseRay, float ___m_mouseRayLength, ushort ___m_lastEditLine, Vector3 ___m_hitPosition)
        {
            try
            {
                var netManager = Singleton<NetManager>.instance;
                ToolBase.RaycastOutput raycastOutput = RayCastWrapper.RayCast(__instance, ___m_prefab, ___m_mouseRay, ___m_mouseRayLength);
                Log.Debug($"{raycastOutput.m_hitPos.x} {raycastOutput.m_hitPos.y} {raycastOutput.m_hitPos.z} on {raycastOutput.m_netSegment}");
                if (___m_prefab.m_transportType.IsSharedStopTransport() && raycastOutput.m_netSegment != 0 && ___m_lastEditLine != 0)
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
    class TransportToolPatch_GetStopPosition
    {
        static void Postfix(ref bool __result, TransportInfo info, ushort segment, ushort building, ushort firstStop, ref Vector3 hitPos)
        {
            //Log.Debug("Called GetStopPosition Postfix");
            if (!__result && segment != 0 && info.m_transportType != TransportInfo.TransportType.Pedestrian)
            {
                Singleton<SharedStopsTool>.instance.GetStopPosition(ref __result, info, segment, building, firstStop, ref hitPos, out bool fixedPlatform);
            }
        }
    }
}
