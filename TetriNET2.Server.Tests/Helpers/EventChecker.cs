using System;
using System.Reflection;

namespace TetriNET2.Server.Tests.Helpers
{
    public static class EventChecker
    {
        public static bool CheckEvents<T>(T instance)
        {
            Type t = instance.GetType();
            EventInfo[] events = t.GetEvents();
            foreach (EventInfo e in events)
            {
                if (e.DeclaringType == null)
                    return false;
                FieldInfo fi = e.DeclaringType.GetField(e.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (fi == null)
                    return false;
                object value = fi.GetValue(instance);
                if (value == null)
                    return false;
            }
            return true;
        }
    }
}
