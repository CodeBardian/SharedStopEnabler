﻿using System;

namespace AdvancedStopSelection.RedirectionFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class RedirectAttribute : Attribute
    {
        protected RedirectAttribute(bool onCreated = false)
        {
            OnCreated = onCreated;
        }

        public bool OnCreated { get; }
    }
}