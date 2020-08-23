using System.Linq;
using ICities;
using SharedStopEnabler.StopSelection;
using SharedStopEnabler.Util;
using System;
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
                Singleton<SharedStopsTool>.instance.Start();

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
    }
}
