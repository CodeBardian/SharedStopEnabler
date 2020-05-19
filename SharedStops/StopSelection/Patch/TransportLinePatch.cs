using ColossalFramework;
using ColossalFramework.PlatformServices;
using HarmonyLib;
using SharedStopEnabler.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SharedStopEnabler.StopSelection.Patch
{
    //[HarmonyPatch(typeof(TransportTool), "AddStop")]
    //class TransportLinePatch1
    //{
    //    static bool Prefix()
    //    {
    //        //Log.Debug($"Transportline {lineID} segment: {segment}, stops {stops}, laststop {laststop}");
    //        return true;
    //    }

    //    static void Postfix(ref IEnumerator __result)
    //    {
    //        Log.Debug("Transportline added stop");
    //    }
    //}


    [HarmonyPatch(typeof(TransportTool), "GetStopPosition")]
    class TransportToolPatch2
    {
        static bool Prefix(out bool __state, ref bool __result, TransportInfo info, ushort segment, ushort building, ushort firstStop, ref Vector3 hitPos, out bool fixedPlatform)
        {
            __state = false;

            Log.Debug($"segment: {segment}");
            //fixedPlatform = false;
            //return true;

            if (Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment].HasSharedStop(segment, info.m_stopFlag))
            {
                __state = true;
            }


            __result = Singleton<SharedStopsTool>.instance.GetStopPosition(info, segment, building, firstStop, ref hitPos, out fixedPlatform);
            return false;
        }

        static void Postfix(bool __state, ref bool __result)
        {
            if (__result && __state)
            {
                //TODO: add sharedstop segemnt and remove props #2
                Log.Debug("Created SharedStop");
            }
        }
    }
}
