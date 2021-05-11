using ColossalFramework;
using HarmonyLib;
using SharedStopEnabler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SharedStopEnabler.StopSelection.Patch
{
    [HarmonyPatch(typeof(TransportManager), "ReleaseLine", new Type[] { typeof(ushort) })]
    class TransportManagerPatch_ReleaseLine
    {
        static bool Prefix(ushort lineID)
        {
            TransportLine line = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID];
            if (lineID == 0 || !line.Info.m_transportType.IsSharedStopTransport()) return true;

            Log.Debug($"remove line {lineID} with stops {line.m_stops}");
            ushort stops = line.m_stops;
            if (stops == 0) return true;
            for (int i = 0; i < line.CountStops(lineID); i++)
            {
                stops = TransportLine.GetNextStop(stops);
                uint lane = Singleton<NetManager>.instance.m_nodes.m_buffer[stops].m_lane;
                ushort segment = Singleton<NetManager>.instance.m_lanes.m_buffer[lane].m_segment;
                if (lane != 0 && segment != 0)
                {
                    Log.Debug($"remove stop {stops} on lane {lane} on segment {segment}");
                    Singleton<SharedStopsTool>.instance.RemoveSharedStop(segment, lineID, lane);
                }
            }
            return true;
        }
    }
}
