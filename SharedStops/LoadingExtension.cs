using System.Linq;
using ColossalFramework.UI;
using ICities;
using UnityEngine;
using AdvancedStopSelection.StopSelection;
using AdvancedStopSelection.RedirectionFramework;
using AdvancedStopSelection.Detour;

namespace AdvancedStopSelection
{
    public class SharedStopsLoadingExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            Redirector<TransportToolDetour>.Deploy();

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
                //bool hasStops = network.m_lanes.Any(lane => lane.m_stopType != VehicleInfo.VehicleType.None);  //why doesnt that work
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

        public override void OnLevelUnloading()
        {
            Redirector<TransportToolDetour>.Revert();
            base.OnLevelUnloading();
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
