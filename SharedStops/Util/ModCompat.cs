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

                    ulong modID = mod.publishedFileID.AsUInt64;
                    Log.Info($"found mod {modID} {mod.name} {((IUserMod)mod.userModInstance).Name}");
                    if (modID != ulong.MaxValue)
                    {
                        foundMods.Add(modID, mod);                  
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"scan mods error: {e}");
            }
        }
    }
}
