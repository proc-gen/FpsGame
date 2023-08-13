using Newtonsoft.Json.Linq;
using System;
using System.Reflection;


namespace FpsGame.Common.Serialization.ComponentConverters
{
    public abstract class JObjectConverter : Converter
    {
        protected Type DeserializedType;

        public JObjectConverter() { }

        protected T Convert<T>(JObject data)
            where T : new()
        {
            object retVal = Activator.CreateInstance<T>();

            if (DeserializedType == null)
            {
                DeserializedType = typeof(T);
            }

            PropertyInfo[] properties = DeserializedType.GetProperties();
            FieldInfo[] fields = DeserializedType.GetFields();

            TypedReference tRef = __makeref(retVal);

            foreach (var pi in properties)
            {
                if (data[pi.Name] != null && pi.Name != "IsChanged")
                {
                    pi.SetValue(retVal, System.Convert.ChangeType(data[pi.Name], pi.PropertyType));
                }
            }

            foreach (var fi in fields)
            {
                if (data[fi.Name] != null)
                {
                    fi.SetValueDirect(tRef, System.Convert.ChangeType(data[fi.Name], fi.FieldType));
                }
            }

            return (T)retVal;
        }
    }
}
