﻿using ColossalFramework;
using ICities;
using AdvancedStopSelection.StopSelection;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedStopSelection.StopSelection
{
    /// <summary>
    /// Notifies its listeners when new transport line is added
    /// </summary>
    public class TransportLineObserver : ThreadingExtensionBase
    {
        //private readonly ColorAssigner m_colorAssigner;
        private TransportManager m_transportManager;
        private HashSet<ushort> m_registeredTransportLineIDs;

        /// <summary>
        /// Triggered when creation of new TransportLine is started
        /// </summary>
        //public event Action<ushort, TransportLine> NewTransportLine;

        //public TransportLineObserver() : base()
        //{
        //    //m_colorAssigner = new ColorAssigner(this);
        //}

        public override void OnCreated(IThreading threading)
        {
            m_transportManager = Singleton<TransportManager>.instance;
            m_registeredTransportLineIDs = new HashSet<ushort>();
            Debug.Log($"Created TransportLineObserver");
            base.OnCreated(threading);
        }

        /// <summary>
        /// Monitors TransportLine count and raises <see cref="NewTransportLine"/> event when new TransportLine is created.
        /// </summary>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            // Noticed new TransportLine
            if (SimulationTransportLineCount > RegisteredTransportLineCount)
            {
                ForEachTransportLine((lineID, transportLine) =>
                {
                    bool newLineWasRegistered = RegisterIfCreated(lineID, transportLine);
                    if (newLineWasRegistered)
                    {
                        Debug.Log($"Added transport line {lineID} flags {transportLine.m_flags}");
                        Debug.Log($"Avoid same stop {transportLine.Info.m_avoidSameStopPlatform}");

                        transportLine.Info.m_avoidSameStopPlatform = false;
                        Debug.Log($"Avoid same stop {transportLine.Info.m_avoidSameStopPlatform}");
                        //NewTransportLine(lineID, transportLine);
                    }
                });
            }
            // TransportLine was removed
            if (SimulationTransportLineCount < RegisteredTransportLineCount)
            {
                Debug.Log($"Removed transport line");

                m_registeredTransportLineIDs.Clear();

                // Collect all lines that are still present
                ForEachTransportLine((lineID, transportLine) =>
                {
                    RegisterIfCreated(lineID, transportLine);
                });
            }
            //base.OnUpdate(realTimeDelta, simulationTimeDelta);
        }

        /// <summary>
        /// Iterate over all TransportLines calling action function for each line.
        /// </summary>
        /// <param name="action">(lineID:ushort * transportLine:TransportLine) : ()</param>
        private void ForEachTransportLine(Action<ushort, TransportLine> action)
        {
            var lines = m_transportManager.m_lines;
            for (ushort lineID = 0; lineID < lines.m_size; lineID++)
            {
                action(lineID, lines.m_buffer[lineID]);
            }
        }

        /// <summary>
        /// Register transportLine if it has flag <see cref="TransportLine.Flags.Created"/>
        /// </summary>
        /// <returns>True if new line was registered</returns>
        private bool RegisterIfCreated(ushort lineID, TransportLine transportLine)
        {
            if (transportLine.FlagSet(TransportLine.Flags.Created))
            {
                return m_registeredTransportLineIDs.Add(lineID);
            }

            return false;
        }

        private int SimulationTransportLineCount
        {
            get { return m_transportManager.m_lineCount; }
        }

        private int RegisteredTransportLineCount
        {
            get { return m_registeredTransportLineIDs.Count; }
        }
    }
}
