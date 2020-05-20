using System.Linq;
using ColossalFramework.UI;
using ICities;
using UnityEngine;
using SharedStopEnabler.StopSelection;
using SharedStopEnabler.Util;
using System;
using System.Reflection;
using ColossalFramework;
using CitiesHarmony.API;

namespace SharedStopEnabler
{
    public class SharedStopsLoadingExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            Log.Info($"OnLevelLoaded: {mode}");
            base.OnLevelLoaded(mode);

            try
            {
                if (!Singleton<SharedStopsTool>.exists)
                    Singleton<SharedStopsTool>.Ensure();

                if (HarmonyHelper.IsHarmonyInstalled)
                {
                    Patcher.PatchAll();
                    Log.Info("Patches deployed");
                }
                else Log.Info("Harmony not found");
            }
            catch (Exception e)
            {
                Log.Error($"Failed deploying Patches: {e}");
            }

            try
            {
                EnableElevatedStops();
                Log.Info($"elevated stops enabled");
            }
            catch (Exception e)
            {
                Log.Error($"Failed enabling elevated stops: {e}");
            }
        }
        public override void OnLevelUnloading()
        {
            try
            {
                if (HarmonyHelper.IsHarmonyInstalled)
                {
                    Patcher.UnpatchAll();
                    Log.Info("patching reverted");
                }
                else Log.Info("Harmony not found");
            }
            catch (Exception e)
            {
                Log.Error($"Failed reverting patches: {e}");
            }
            base.OnLevelUnloading();
            Log.Info("level unloaded");

        }

        public override void OnReleased()
        {
            UnityEngine.Object.DestroyImmediate(Singleton<SharedStopsTool>.instance);
        }

        private void EnableElevatedStops()
        {
            NetInfo[] networks = Resources.FindObjectsOfTypeAll<NetInfo>();
            foreach (var network in networks)
            {
                if (!network.m_hasPedestrianLanes) continue;
                RoadAI ai = network.m_netAI as RoadAI;
                if (ai == null) continue;
                bool hasStops = false;
                foreach (NetInfo.Lane lane in network.m_lanes)
                {
                    if (lane.m_stopType != VehicleInfo.VehicleType.None)
                    {
                        hasStops = true;
                        break;
                    }
                }
                if (!hasStops) continue;

                VehicleInfo.VehicleType firstStopType = network.m_lanes[network.m_sortedLanes[0]].m_stopType;
                VehicleInfo.VehicleType secondStopType = network.m_lanes[network.m_sortedLanes[network.m_sortedLanes.Length - 1]].m_stopType;
                VehicleInfo.VehicleType middleStopType = VehicleInfo.VehicleType.None;

                for (int i = 1; i < network.m_lanes.Length - 2; i++)
                {
                    if (network.m_lanes[network.m_sortedLanes[i]].m_laneType == NetInfo.LaneType.Pedestrian)
                    {
                        middleStopType = network.m_lanes[network.m_sortedLanes[i]].m_stopType;
                        break;
                    }
                }
                EnableStops(ai.m_elevatedInfo, firstStopType, middleStopType, secondStopType);
                EnableStops(ai.m_bridgeInfo, firstStopType, middleStopType, secondStopType);
                EnableStops(ai.m_slopeInfo, firstStopType, middleStopType, secondStopType);
                EnableStops(ai.m_tunnelInfo, firstStopType, middleStopType, secondStopType);
            }
        }

        private static void EnableStops(NetInfo info, VehicleInfo.VehicleType firstStopType, VehicleInfo.VehicleType middleStopType, VehicleInfo.VehicleType secondStopType)
        {
            if (info == null) return;
            for (int i = 1; i < info.m_lanes.Length - 2; i++)
            {
                if (info.m_lanes[info.m_sortedLanes[i]].m_vehicleType == VehicleInfo.VehicleType.None)
                {
                    info.m_lanes[info.m_sortedLanes[i]].m_stopType = middleStopType;
                }
            }
            info.m_lanes[info.m_sortedLanes[0]].m_stopType = firstStopType;
            info.m_lanes[info.m_sortedLanes[0]].m_stopOffset = 0f;
            info.m_lanes[info.m_sortedLanes[info.m_sortedLanes.Length - 1]].m_stopType = secondStopType;
            info.m_lanes[info.m_sortedLanes[info.m_sortedLanes.Length - 1]].m_stopOffset = 0f;
        }
    }
}
