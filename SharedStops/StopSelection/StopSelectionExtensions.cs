﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace AdvancedStopSelection.StopSelection
{
    public static class StopSelectionExtensions
    {
        public static bool FlagSet(this TransportLine transportLine, TransportLine.Flags flag)
        {
            return (transportLine.m_flags & flag) == flag;
        }

        public static void ForEachLine(this TransportLine transportLine, ushort lineID)
        {
           
        }
    }
}
