using SharedStopEnabler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SharedStopEnabler.StopSelection
{
    static class StopsUtil
    {
        public static void EnableElevatedStops()
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
            try
            {
                if (info == null) return;
                if (info.m_lanes.Length == 0 || info.m_sortedLanes.Length == 0) 
                {
                    Log.Error($"[SSE] custom road {info} without lanes found. Can't enable Shared Stops!");
                    return; 
                }
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
            catch (Exception e)
            {
                Log.Error($"Failed on EnableStops {e}");
            }
        }

        public static void InitLaneProps(string propName)
        {
            for (uint i = 0; i < PrefabCollection<NetInfo>.LoadedCount(); i++)
            {
                var netInfo = PrefabCollection<NetInfo>.GetLoaded(i);

                if (netInfo == null || netInfo.m_lanes == null || !netInfo.m_hasPedestrianLanes) continue;

                foreach (NetInfo.Segment segment in netInfo.m_segments)
                {
                    if (segment == null) continue;

                    if (segment.m_lodMaterial.name.Contains("BusSide"))
                    {
                        segment.m_backwardForbidden &= ~NetSegment.Flags.StopRight2;
                        segment.m_forwardForbidden &= ~NetSegment.Flags.StopLeft2;
                    }
                    else if (segment.m_lodMaterial.name.Contains("TramAndBusStop"))
                    {
                        segment.m_backwardForbidden &= ~NetSegment.Flags.StopLeft2;
                        segment.m_forwardForbidden &= ~NetSegment.Flags.StopRight2;
                    }
                    else if (segment.m_lodMaterial.name.Contains("BusBoth"))
                    {
                        segment.m_backwardForbidden &= ~NetSegment.Flags.StopBoth2;
                        segment.m_forwardForbidden &= ~NetSegment.Flags.StopBoth2;
                    }
                }

                foreach (NetInfo.Lane lane in netInfo.m_lanes)
                {
                    if (lane == null || lane.m_laneType != NetInfo.LaneType.Pedestrian || lane.m_laneProps == null || lane.m_laneProps.m_props == null) continue;

                    foreach (NetLaneProps.Prop laneProp in lane.m_laneProps.m_props)
                    {
                        if (laneProp != null && laneProp.m_prop != null && laneProp.m_prop.name == propName)
                        {
                            laneProp.m_flagsForbidden |= NetLane.Flags.Stop;
                        }
                    }
                }
            }
        }

        public static void ReplaceLaneProp(string original, string replacement)
        {
            for (uint i = 0; i < PrefabCollection<NetInfo>.LoadedCount(); i++)
            {
                var netInfo = PrefabCollection<NetInfo>.GetLoaded(i);
                if (netInfo == null)
                {
                    Log.Info("SSE: The name '" + netInfo + "'does not belong to a loaded net!");
                    return;
                }
                if (!netInfo.m_hasPedestrianLanes) continue;

                var replacementProp = PrefabCollection<PropInfo>.FindLoaded(replacement);
                if (replacementProp == null)
                {
                    Log.Info("SSE:The name '" + replacement + "'does not belong to a loaded prop!");
                    return;
                }

                if (netInfo.m_lanes != null)
                {
                    foreach (var lane in netInfo.m_lanes)
                    {
                        if (lane != null && lane.m_laneProps != null && lane.m_laneProps.m_props != null)
                        {
                            foreach (var laneProp in lane.m_laneProps.m_props)
                            {
                                if (laneProp != null && laneProp.m_prop != null && laneProp.m_prop.name == original)
                                {
                                    laneProp.m_prop = replacementProp;
                                    laneProp.m_finalProp = replacementProp;

                                    switch (lane.m_laneProps.name)
                                    {
                                        case "Props - Gravel Left":
                                            laneProp.m_angle = -90;
                                            break;
                                        case "Props - Gravel Right":
                                            laneProp.m_angle = 90;
                                            break;
                                        case "Props - Basic Right":
                                            laneProp.m_angle = 180;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
