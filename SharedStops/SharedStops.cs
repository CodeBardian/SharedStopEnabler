using System;
using System.Collections.Generic;
using System.Linq;
using ICities;
using UnityEngine;

namespace AdvancedStopSelection
{
    public class SharedStops : IUserMod
    {
        public string Name => "Advanced Stop Selection";

        public string Description => "Shared Public Transport Stops for buses and trams + advanced stop selection for trains and metros (shift-click)";
    }
}

//TODO: bus stops on tram stops/ bus stops on lefts
