using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedStopEnabler.StopSelection
{
    public static class EnumHelper
    {
        static EnumHelper()
        {
            Dict = Enum.GetNames(typeof(SharedStopSegment.SharedStopTypes)).ToDictionary(x => x, x => (SharedStopSegment.SharedStopTypes)Enum.Parse(typeof(SharedStopSegment.SharedStopTypes), x), StringComparer.OrdinalIgnoreCase);
        }

        private static readonly Dictionary<string, SharedStopSegment.SharedStopTypes> Dict;

        public static SharedStopSegment.SharedStopTypes Convert(this TransportInfo.TransportType transportType)
        {
            return Dict[transportType.ToString()];
        }
    }
}
