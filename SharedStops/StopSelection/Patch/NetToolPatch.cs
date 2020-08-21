using ColossalFramework;
using HarmonyLib;
using SharedStopEnabler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SharedStopEnabler.StopSelection.Patch
{
    [HarmonyPatch(typeof(NetTool))]
    [HarmonyPatch("CreateNode")]
    [HarmonyPatch(
        new Type[] { typeof(NetInfo), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint),
            typeof(FastList<NetTool.NodePosition>), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool),
            typeof(bool), typeof(ushort), typeof(ushort), typeof(ushort), typeof(int), typeof(int)},
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
            ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
            ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out})]
    class NetToolPatch_CreateNode
    {
        static void Postfix(NetTool.ControlPoint middlePoint, ref ushort segment)
        {
            if (middlePoint.m_segment == 0 || segment == 0) return;
            NetSegment data = Singleton<NetManager>.instance.m_segments.m_buffer[middlePoint.m_segment];
            if (data.IsSharedStopSegment(middlePoint.m_segment))
            {
                Singleton<SharedStopsTool>.instance.sharedStopSegments.First(s => s.m_segment == middlePoint.m_segment).m_segment = segment;
                Log.Debug($"Updated sharedstopsegment {middlePoint.m_segment} to {segment}");
            }
        }
    }
}
