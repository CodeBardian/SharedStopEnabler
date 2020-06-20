using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

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

        public static TA As<TA>() where TA : RoadBridgeAI
        {
            var type = typeof(TA);
            TA instance = (TA)Activator.CreateInstance(type);

            PropertyInfo[] properties = type.GetProperties();
            foreach (var property in properties)
            {
                property.SetValue(instance, property.GetValue(instance, null), null);
            }

            return (TA)instance;
        }
    }
}
