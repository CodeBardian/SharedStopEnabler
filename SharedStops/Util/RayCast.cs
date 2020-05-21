using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SharedStopEnabler.Util
{
    static class RayCastWrapper
    {
        public static ToolBase.RaycastOutput RayCast(TransportTool __instance, TransportInfo ___m_prefab, Ray ___m_mouseRay, float ___m_mouseRayLength)
        {
            ToolBase.RaycastInput input = new ToolBase.RaycastInput(___m_mouseRay, ___m_mouseRayLength)
            {
                m_buildingService = new ToolBase.RaycastService(___m_prefab.m_stationService, ___m_prefab.m_stationSubService, ___m_prefab.m_stationLayer),
                m_netService = new ToolBase.RaycastService(___m_prefab.m_netService, ___m_prefab.m_netSubService, ___m_prefab.m_netLayer),
                m_netService2 = new ToolBase.RaycastService(___m_prefab.m_secondaryNetService, ___m_prefab.m_secondaryNetSubService, ___m_prefab.m_netLayer),
                m_ignoreTerrain = true,
                m_ignoreSegmentFlags = ((___m_prefab.m_netService == ItemClass.Service.None) ? NetSegment.Flags.All : NetSegment.Flags.None),
                m_ignoreBuildingFlags = ((___m_prefab.m_stationService == ItemClass.Service.None && ___m_prefab.m_transportType != TransportInfo.TransportType.Pedestrian) ? Building.Flags.All : Building.Flags.None)
            };
            Log.Debug($"SSE: input set {__instance}");
            object[] parameters = new object[] { input, null };
            object result = typeof(ToolBase).GetMethod("RayCast", BindingFlags.Static | BindingFlags.NonPublic).Invoke(__instance, parameters);
            Log.Debug($"SSE: invoke passed {result}");
            ToolBase.RaycastOutput raycastOutput = default;
            if ((bool)result)
                raycastOutput = (ToolBase.RaycastOutput)parameters[1];
            return raycastOutput;
        }
    }
}
