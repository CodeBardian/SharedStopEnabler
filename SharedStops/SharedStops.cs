using System;
using System.Collections.Generic;
using System.Linq;
using ICities;
using SharedStopEnabler.Util;
using UnityEngine;

namespace SharedStopEnabler
{
    public class SharedStops : IUserMod
    {
        public string Name => "Shared Stop Enabler";

        public string Description => "Shared Public Transport Stops";

        public static string ModVersion => typeof(SharedStops).Assembly.GetName().Version.ToString();

        //public void OnSettingsUI(UIHelperBase helper)
        //{
        //    //TODO: settings menu
        //}

        public void OnEnabled()
        {
            Log.Info($"SharedStops enabled {ModVersion}");
        }

        public void OnDisabled()
        {
            Log.Info($"SharedStops disabled");
        }
    }
}
