using HarmonyLib;
using SharedStopEnabler.StopSelection.Patch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SharedStopEnabler.Util
{
    public static class Patcher
    {
        private const string HarmonyId = "com.codebard.sharedstops";
        private static bool patched = false;

        public static void PatchAll()
        {
            if (patched) return;

            patched = true;
            var harmony = new Harmony(HarmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            ModCompat.ScanMods();

            if (ModCompat.foundMods.ContainsKey(1394468624) && ModCompat.foundMods[1394468624].isEnabled)
            {
                Log.Info("AdvancedStopSelection found. Applying patch...");

                MethodInfo original = Type.GetType("ImprovedStopSelection.Detour.TransportToolDetour, ImprovedStopSelection").GetMethod("GetStopPosition", BindingFlags.Instance | BindingFlags.NonPublic);
                Log.Info($"{original}");
                MethodInfo postfix = typeof(TransportToolPatch_GetStopPosition).GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic);

                Log.Info($"{original}, {postfix}");

                harmony.Patch(original, postfix: new HarmonyMethod(postfix));
            }
        }

        public static void UnpatchAll()
        {
            if (!patched) return;

            var harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId);
            patched = false;
        }
    }
}
