using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SharedStopEnabler.Util
{
    static class Util
    {
        public static T DeepClone<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        public static TA As<TA>(this RoadBridgeAI roadBridge) where TA : RoadBridgeAI
        {
            var type = typeof(TA);
            var instance = Activator.CreateInstance(type);

            PropertyInfo[] properties = type.GetProperties();
            foreach (var property in properties)
            {
                property.SetValue(instance, property.GetValue(roadBridge, null), null);
            }

            return (TA)instance;
        }
    }
}
