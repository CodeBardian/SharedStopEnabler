using ColossalFramework;
using HarmonyLib;
using SharedStopEnabler.Util;
using System;
using System.Linq;
using UnityEngine;

namespace SharedStopEnabler.StopSelection.Patch
{
    [HarmonyPatch(typeof(TransportLine), "AddStop")]
    class TransportLinePatch_AddStop
    {
        static void Postfix(ushort ___m_stops, ushort lineID, Vector3 position)
        {
            NetNode stopnode = Singleton<NetManager>.instance.m_nodes.m_buffer[___m_stops];
            Singleton<SharedStopsTool>.instance.m_lastEditPoint = stopnode.m_position;

            TransportLine line = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID];
            if (lineID == 0 || !line.Info.m_transportType.IsSharedStopTransport()) return;
            uint lane = Singleton<NetManager>.instance.m_nodes.m_buffer[___m_stops].m_lane;
            ushort segment = Singleton<NetManager>.instance.m_lanes.m_buffer[lane].m_segment;
            if (Singleton<NetManager>.instance.m_segments.m_buffer[segment].GetClosestLanePosition(position, NetInfo.LaneType.Vehicle, line.Info.m_vehicleType, out _, out _, out int laneindex, out _))
            {
                NetInfo.Direction direction = Singleton<NetManager>.instance.m_segments.m_buffer[segment].Info.m_lanes[laneindex].m_direction;
                Singleton<SharedStopsTool>.instance.AddSharedStop(segment, (SharedStopSegment.SharedStopTypes)Enum.Parse(typeof(SharedStopSegment.SharedStopTypes), line.Info.m_transportType.ToString()), lineID, direction);
            }
        }
    }

    [HarmonyPatch(typeof(TransportLine), "RemoveStop", new Type[] { typeof(ushort), typeof(int) })]
    class TransportLinePatch_RemoveStop
    {
        static bool Prefix(ushort lineID, int index)
        {
            TransportLine line = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID];
            if (lineID == 0 || !line.Info.m_transportType.IsSharedStopTransport()) return true;

            ushort stop = line.GetStop(index);
            uint lane = Singleton<NetManager>.instance.m_nodes.m_buffer[stop].m_lane;
            Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[stop].m_position;
            ushort segment = Singleton<NetManager>.instance.m_lanes.m_buffer[lane].m_segment;
            if (Singleton<SharedStopsTool>.instance.sharedStopSegments != null && !Singleton<SharedStopsTool>.instance.sharedStopSegments.Any(s => s.m_segment == segment)) return true;
            if (Singleton<NetManager>.instance.m_segments.m_buffer[segment].GetClosestLanePosition(position, NetInfo.LaneType.Vehicle, line.Info.m_vehicleType, out _, out _, out int laneindex, out _))
            {
                NetInfo.Direction direction = Singleton<NetManager>.instance.m_segments.m_buffer[segment].Info.m_lanes[laneindex].m_direction;
                Singleton<SharedStopsTool>.instance.RemoveSharedStop(segment, (SharedStopSegment.SharedStopTypes)Enum.Parse(typeof(SharedStopSegment.SharedStopTypes), line.Info.m_transportType.ToString()), lineID, direction);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(TransportLine), "MoveStop", new Type[] { typeof(ushort), typeof(int), typeof(Vector3), typeof(bool) })]
    class TransportLinePatch_MoveStop
    {
        static bool Prefix(ushort lineID, int index, Vector3 newPos, bool fixedPlatform)
        {
            TransportLine line = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID];
            if (lineID == 0 || !line.Info.m_transportType.IsSharedStopTransport()) return true;

            ushort stop = line.GetStop(index);
            uint lane = Singleton<NetManager>.instance.m_nodes.m_buffer[stop].m_lane;
            Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[stop].m_position;
            ushort segment = Singleton<NetManager>.instance.m_lanes.m_buffer[lane].m_segment;
            if (!Singleton<SharedStopsTool>.instance.sharedStopSegments.Any(s => s.m_segment == segment)) return true;
            Log.Debug($"MoveStop: line {lineID} from segment {segment}");
            if (Singleton<NetManager>.instance.m_segments.m_buffer[segment].GetClosestLanePosition(position, NetInfo.LaneType.Vehicle, line.Info.m_vehicleType, out _, out _, out int laneindex, out _))
            {
                NetInfo.Direction direction = Singleton<NetManager>.instance.m_segments.m_buffer[segment].Info.m_lanes[laneindex].m_direction;
                Singleton<SharedStopsTool>.instance.RemoveSharedStop(segment, (SharedStopSegment.SharedStopTypes)Enum.Parse(typeof(SharedStopSegment.SharedStopTypes), line.Info.m_transportType.ToString()), lineID, direction);
            }
            return true;
        }
    }
}
