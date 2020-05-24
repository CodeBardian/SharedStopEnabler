using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedStopEnabler.StopSelection
{
    using ColossalFramework;
    using ICities;
    using SharedStopEnabler.Util;
    using System;
    using System.Collections.Generic;
    public class TransportLineObserver : ThreadingExtensionBase
    {
        private HashSet<ushort> m_registeredTransportLineIDs;

        public override void OnCreated(IThreading threading)
        {
            m_registeredTransportLineIDs = new HashSet<ushort>();

            base.OnCreated(threading);
        }

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            // Noticed new TransportLine
            if (SimulationTransportLineCount > m_registeredTransportLineIDs.Count)
            {
                ForEachTransportLine((lineID, transportLine) =>
                {
                    bool newLineWasRegistered = RegisterIfCreated(lineID, transportLine);
                    if (newLineWasRegistered)
                        Log.Debug($"Added transport line {lineID} flags {transportLine.m_flags}");
                });
            }
            // TransportLine was removed
            else if (SimulationTransportLineCount < m_registeredTransportLineIDs.Count)
            {
                var temp = new HashSet<ushort>(m_registeredTransportLineIDs);
                m_registeredTransportLineIDs.Clear();

                ForEachTransportLine((lineID, transportLine) =>
                {
                    RegisterIfCreated(lineID, transportLine);
                });
                temp.ExceptWith(m_registeredTransportLineIDs);
                foreach (var line in temp)
                {
                    //if ((Singleton<TransportManager>.instance.m_lines.m_buffer[line].m_flags & TransportLine.Flags.Selected) != TransportLine.Flags.Selected) continue;
                    var transportType = Singleton<TransportManager>.instance.m_lines.m_buffer[line].Info.m_transportType;
                    Log.Debug($"Removed transport line {line}");
                    var sharedSegments = StopSelectionExtensions.FindSharedStopsByLine(line);
                    Log.Debug($"Removed found sharedsegments {sharedSegments.Count}");
                    if (sharedSegments.Count != 0)
                    {
                        var direction = sharedSegments[0].m_lines[line];
                        Log.Debug($"Removed direction {direction} on segment {sharedSegments[0].m_segment}");
                        Singleton<SharedStopsTool>.instance.RemoveSharedStop(sharedSegments[0].m_segment, (SharedStopSegment.SharedStopTypes)Enum.Parse(typeof(SharedStopSegment.SharedStopTypes), transportType.ToString()), line, direction);
                    }
                }
            }
        }

        private void ForEachTransportLine(Action<ushort, TransportLine> action)
        {
            var lines = Singleton<TransportManager>.instance.m_lines;
            for (ushort lineID = 0; lineID < lines.m_size; lineID++)
            {
                action(lineID, lines.m_buffer[lineID]);
            }
        }

        private bool RegisterIfCreated(ushort lineID, TransportLine transportLine)
        {
            if (transportLine.FlagSet(TransportLine.Flags.Created))
            {
                return m_registeredTransportLineIDs.Add(lineID);
            }

            return false;
        }

        private int SimulationTransportLineCount => Singleton<TransportManager>.instance.m_lineCount;
    }
}
