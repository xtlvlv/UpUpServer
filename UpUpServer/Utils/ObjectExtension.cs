
using System.Reflection;
using Newtonsoft.Json;

namespace UpUpServer
{
    public static class ObjectExtension
    {
        public static T DeepCloneByJson<T>(this T source) where T : new()
        {
            if (source == null)
            {
                return default(T);
            }
            string json = JsonConvert.SerializeObject(source);
            T destination = JsonConvert.DeserializeObject<T>(json);
            return destination;
        }

        public static T DeepCloneByReflection<T>(this T source) where T : new()
        {
            if (source == null)
            {
                return default(T);
            }
            var destination = (T) DeepCopy(source);
            return destination;
        }
        
        // public static object DeepCopy(object obj, ISet<string> excludeName = null)
        public static object DeepCopy(object obj, ISet<Type> excludeTypes = null)
        {
            if (obj == null)
            {
                return null;
            }

            Type type = obj.GetType();

            // int bool char 
            if (type.IsPrimitive || type == typeof(string))
            {
                return obj;
            }

            if (excludeTypes != null && excludeTypes.Contains(type))
            {
                return obj;
            }

            if (type.IsArray)
            {
                Type elementType = Type.GetType(
                    type.FullName.Replace("[]", string.Empty));
                var   array       = obj as Array;
                Array copiedArray = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    copiedArray.SetValue(DeepCopy(array.GetValue(i), excludeTypes), i);
                }
                return Convert.ChangeType(copiedArray, obj.GetType());
            }

            object instance = Activator.CreateInstance(obj.GetType());
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object fieldValue = field.GetValue(obj);
                if (fieldValue == null)
                {
                    continue;
                }
                field.SetValue(instance, DeepCopy(fieldValue, excludeTypes));
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0))
            {
                try
                {
                    object propertyValue = property.GetValue(obj, null);
                    property.SetValue(instance, DeepCopy(propertyValue, excludeTypes), null);
                }
                catch
                {
                    Log.Warning("Can't copy property: " + property.Name);
                }
            }

            return instance;
        }
    }
}