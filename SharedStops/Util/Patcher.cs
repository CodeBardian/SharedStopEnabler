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

            Log.Info("Manual patching TransportTool_GetStopPosition...");
            MethodInfo gspMethod = typeof(TransportTool).GetMethod("GetStopPosition", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo gspTranspiler = typeof(TransportToolPatch_GetStopPosition).GetMethod("Transpiler", BindingFlags.Static | BindingFlags.NonPublic);

            if (ModCompat.foundMods.ContainsKey(1394468624) && ModCompat.foundMods[1394468624].isEnabled)
            {
                Log.Info("AdvancedStopSelection found. Applying patch...");
                Type type = Assembly.Load("ImprovedStopSelection").GetType("ImprovedStopSelection.Detour.TransportToolDetour");
                Log.Info($"{type}");
                gspMethod = type.GetMethod("GetStopPosition", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            Log.Info($"{gspMethod}, {gspTranspiler}");
            harmony.Patch(gspMethod, transpiler: new HarmonyMethod(gspTranspiler));
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
