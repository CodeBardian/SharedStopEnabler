using ColossalFramework;
using HarmonyLib;
using SharedStopEnabler.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedStopEnabler.StopSelection.Patch
{
    [HarmonyPatch(typeof(TransportLine), "AddStop")]
    class TransportLinePatch_AddStop
    {
        static void Postfix(TransportLine __instance, ushort ___m_stops)
        {
            NetNode stopnode = Singleton<NetManager>.instance.m_nodes.m_buffer[___m_stops];
            Singleton<SharedStopsTool>.instance.m_lastEditPoint = stopnode.m_position;
        }
    }
}
