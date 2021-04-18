using ColossalFramework;
using HarmonyLib;
using SharedStopEnabler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedStopEnabler.StopSelection.Patch
{
    [HarmonyPatch(typeof(TransportLineAI), "AddLaneConnection")]
    class TransportLineAIPatch_AddLaneConnection
    {
        static bool Prefix(NetLane.Flags ___m_stopFlag, VehicleInfo.VehicleType ___m_vehicleType, ushort nodeID, ref NetNode data, uint laneID, byte offset)
        {
            if (nodeID == 0 || !___m_vehicleType.IsSharedStopTransport()) return true;

            ushort segment = Singleton<NetManager>.instance.m_lanes.m_buffer[laneID].m_segment;
            ushort lineID = Singleton<NetManager>.instance.m_nodes.m_buffer[nodeID].m_transportLine;
            TransportLine line = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID];
            Log.Debug($"creating stop on segment {segment}, line {lineID}");
            if (Singleton<NetManager>.instance.m_segments.m_buffer[segment].GetClosestLanePosition(data.m_position, NetInfo.LaneType.Vehicle, ___m_vehicleType, out _, out _, out int laneindex, out _))
            {
                NetInfo.Direction direction = Singleton<NetManager>.instance.m_segments.m_buffer[segment].Info.m_lanes[laneindex].m_direction;
                Singleton<SharedStopsTool>.instance.AddSharedStop(segment, line.Info.m_transportType.Convert(), lineID, direction);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(TransportLineAI), "RemoveLaneConnection")]
    class TransportLineAIPatch_RemoveLaneConnection
    {
        static bool Prefix(NetLane.Flags ___m_stopFlag, VehicleInfo.VehicleType ___m_vehicleType, ushort nodeID, ref NetNode data)
        {
            if (nodeID == 0 || !___m_vehicleType.IsSharedStopTransport()) return true;
            ushort segment = Singleton<NetManager>.instance.m_lanes.m_buffer[data.m_lane].m_segment;
            Log.Debug($"RemoveLaneConn on segment {segment}");

            ushort lineID = Singleton<NetManager>.instance.m_nodes.m_buffer[nodeID].m_transportLine;
            TransportLine line = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID];
            if (Singleton<SharedStopsTool>.instance.sharedStopSegments != null && !Singleton<SharedStopsTool>.instance.sharedStopSegments.Any(s => s.m_segment == segment)) return true;
            if (Singleton<NetManager>.instance.m_segments.m_buffer[segment].GetClosestLanePosition(data.m_position, NetInfo.LaneType.Vehicle, line.Info.m_vehicleType, out _, out _, out int laneindex, out _))
            {
                NetInfo.Direction direction = Singleton<NetManager>.instance.m_segments.m_buffer[segment].Info.m_lanes[laneindex].m_direction;
                Singleton<SharedStopsTool>.instance.RemoveSharedStop(segment, line.Info.m_transportType.Convert(), lineID, direction);
            }
            return true;
        }
    }
}
