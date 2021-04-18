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
    class TransportToolPatch_GetStopPosition
    {
        static void Postfix(ref bool __result, TransportInfo info, ushort segment, ushort building, ushort firstStop, ref Vector3 hitPos, ref bool fixedPlatform)
        {
            //Log.Debug("Called GetStopPosition Postfix");
            if (!__result && segment != 0 && info.m_transportType != TransportInfo.TransportType.Pedestrian)
            {
                Singleton<SharedStopsTool>.instance.GetStopPosition(ref __result, info, segment, building, firstStop, ref hitPos, out fixedPlatform);
            }
        }
    }
}
