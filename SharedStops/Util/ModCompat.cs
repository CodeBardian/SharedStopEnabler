using ColossalFramework;
using ColossalFramework.Plugins;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using static ColossalFramework.Plugins.PluginManager;

namespace SharedStopEnabler.Util
{
    static class ModCompat
    {
        public static Dictionary<ulong, PluginInfo> foundMods = new Dictionary<ulong, PluginInfo>();

        public static void ScanMods()
        {
            try
            {
                foreach (PluginInfo mod in Singleton<PluginManager>.instance.GetPluginsInfo())
                {
                    if (mod.isBuiltin || mod.isCameraScript) continue;

                    Log.Info($"found mod {mod.publishedFileID.AsUInt64} {mod.name} {((IUserMod)mod.userModInstance).Name}");
                    foundMods.Add(mod.publishedFileID.AsUInt64, mod);                  
                }
            }
            catch (Exception e)
            {
                Log.Error($"scan mods error: {e}");
            }
        }
    }
}
