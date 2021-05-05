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
        static bool Prefix(VehicleInfo.VehicleType ___m_vehicleType, ushort nodeID, ref NetNode data, uint laneID, byte offset)
        {
            if (nodeID == 0 || !___m_vehicleType.IsSharedStopTransport()) return true;

            ushort segment = Singleton<NetManager>.instance.m_lanes.m_buffer[laneID].m_segment;
            ushort lineID = Singleton<NetManager>.instance.m_nodes.m_buffer[nodeID].m_transportLine;
            Singleton<SharedStopsTool>.instance.AddSharedStop(segment, lineID, laneID);

            return true;
        }
    }

    [HarmonyPatch(typeof(TransportLineAI), "RemoveLaneConnection")]
    class TransportLineAIPatch_RemoveLaneConnection
    {
        static bool Prefix(out uint __state, VehicleInfo.VehicleType ___m_vehicleType, ushort nodeID, ref NetNode data)
        {
            __state = 0;
            if (nodeID == 0 || !___m_vehicleType.IsSharedStopTransport()) return true;
            ushort segment = Singleton<NetManager>.instance.m_lanes.m_buffer[data.m_lane].m_segment;

            ushort lineID = Singleton<NetManager>.instance.m_nodes.m_buffer[nodeID].m_transportLine;
            TransportLine line = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID];
            __state = data.m_lane;
            NetLane.Flags flags = (NetLane.Flags)Singleton<NetManager>.instance.m_lanes.m_buffer[data.m_lane].m_flags;
            if (!Singleton<SharedStopsTool>.instance.sharedStopSegments.Any(s => s.m_segment == segment)) return true;
            NetSegment segData = Singleton<NetManager>.instance.m_segments.m_buffer[segment];
            Log.Debug($"RemoveLaneConn on segment {segment}, {segData}");
            Singleton<SharedStopsTool>.instance.RemoveSharedStop(segment, lineID, data.m_lane);

            return true;
        }

        static void Postfix(uint __state)
        {
            if (__state == 0) return;

            NetLane lane = Singleton<NetManager>.instance.m_lanes.m_buffer[__state];

            int index = Singleton<SharedStopsTool>.instance.sharedStopSegments.FindIndex(s => s.m_lanes.Keys.Contains(__state));
            if (index == -1) return;    

            foreach (var line in Singleton<SharedStopsTool>.instance.sharedStopSegments[index].m_lanes[__state])
            {
                NetLane.Flags stopflag = Singleton<TransportManager>.instance.m_lines.m_buffer[line].Info.m_stopFlag;
                if (((NetLane.Flags)lane.m_flags & stopflag) == stopflag) continue;

                lane.m_flags |= (ushort)stopflag;
            }
            Singleton<NetManager>.instance.m_lanes.m_buffer[__state].m_flags = lane.m_flags;
            Log.Debug($"lane flags after update: {(NetLane.Flags)Singleton<NetManager>.instance.m_lanes.m_buffer[__state].m_flags}");

            Singleton<NetManager>.instance.UpdateSegmentFlags(Singleton<NetManager>.instance.m_lanes.m_buffer[__state].m_segment);
            Singleton<NetManager>.instance.UpdateSegmentRenderer(Singleton<NetManager>.instance.m_lanes.m_buffer[__state].m_segment, true);
        }
    }
}
