using System.Linq;
using ColossalFramework.UI;
using ICities;
using UnityEngine;
using SharedStopEnabler.StopSelection;
using SharedStopEnabler.Util;
using System;
using HarmonyLib;
using System.Reflection;
using ColossalFramework;

namespace SharedStopEnabler
{
    public class SharedStopsLoadingExtension : LoadingExtensionBase
    {
        Harmony harmony = new Harmony("com.codebard.sharedstops");

        public override void OnLevelLoaded(LoadMode mode)
        {
            Log.Info($"OnLevelLoaded: {mode}");
            base.OnLevelLoaded(mode);

            try
            {
                if (!Singleton<SharedStopsTool>.exists)
                    Singleton<SharedStopsTool>.Ensure();
                Singleton<SharedStopsTool>.instance.Start();

                var assembly = Assembly.GetExecutingAssembly();
                harmony.PatchAll(assembly);
                Log.Info($"Patches deployed");
            }
            catch (Exception e)
            {
                Log.Error($"Failed deploying Patches: {e}");
            }
        }
        public override void OnLevelUnloading()
        {
            try
            {
                harmony.UnpatchAll("com.codebard.sharedstops");
                Log.Info($"patching reverted");
            }
            catch (Exception e)
            {
                Log.Error($"Failed reverting patches: {e}");
            }
            base.OnLevelUnloading();
            Log.Info($"level unloaded");

        }

        public override void OnReleased()
        {
            UnityEngine.Object.DestroyImmediate(Singleton<SharedStopsTool>.instance);
        }

        private void RemoveLaneProp(NetInfo netInfo, string propName)
        {
            if (netInfo == null || netInfo.m_lanes == null) return;

            foreach (NetInfo.Lane lane in netInfo.m_lanes)
            {
                if (lane == null || lane.m_laneProps == null || lane.m_laneProps.m_props == null) continue;

                foreach (NetLaneProps.Prop laneProp in lane.m_laneProps.m_props)
                {
                    if (laneProp != null && laneProp.m_prop != null && laneProp.m_prop.name == propName)
                    {
                        laneProp.m_prop = null;
                        laneProp.m_finalProp = null;
                    }
                }
            }
        }
    }
}
