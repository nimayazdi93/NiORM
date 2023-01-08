using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace NiORM.Core
{
    public static class ObjectDescriber<T, S> where T : new()
    {
        public static List<string> Properties(T Object)
        {
            Type myType = Object.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
            return props.Select(c => c.Name).ToList();
        }

        public static string SQLFormat(T Object, string Key)
        {
            PropertyInfo myPropertyInfo = Object.GetType().GetProperty(Key);
            var Value = myPropertyInfo.GetValue(Object, null);
            if (Value == null)
            {
                return "null";
            }
            if (Value is string)
                return $"N'{Value}'";
            if (Value is int | Value is float | Value is long | Value is double)
                return Value.ToString();
            if (Value is DateTime time)
                return $"'{time:yyyy-MM-dd HH:mm:ss.ss}'";
            if (Value is bool b)
                return b ? "1" : "0";
            return "";
        }

        public static S GetValue(T Object, string Key)
        {
            PropertyInfo myPropertyInfo = Object.GetType().GetProperty(Key);
            var Value = myPropertyInfo.GetValue(Object, null);
            return (S)Value;
        }

        public static void SetValue(T Object, string Key, S Value)
        {
            try
            {
                PropertyInfo myPropertyInfo = Object.GetType().GetProperty(Key);
                var propertyType = myPropertyInfo.PropertyType;
 
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && Value.ToString() == "")
                {
                    var underlyingType = Nullable.GetUnderlyingType(propertyType);
                    var underlyingTypeCode = GetTypeCodeO(underlyingType);
                    switch (underlyingTypeCode)
                    {


                        case TypeCode.Int32:
                            myPropertyInfo.SetValue(Object, (int?)null);
                            return;
                        case TypeCode.Double:
                            myPropertyInfo.SetValue(Object, (double?)null);
                            return;
                        case TypeCode.DateTime:
                            myPropertyInfo.SetValue(Object, (DateTime?)null);
                            return;
                        case TypeCode.Int64:
                            myPropertyInfo.SetValue(Object, (long?)null);
                            return;
                        case TypeCode.Single:
                            myPropertyInfo.SetValue(Object, (float?)null);
                            return;
                        case TypeCode.String:
                            myPropertyInfo.SetValue(Object, null);
                            return;
                        case TypeCode.Boolean:
                            myPropertyInfo.SetValue(Object, (bool?)null);
                            return;
                        case TypeCode.Object:
                            myPropertyInfo.SetValue(Object, null);
                            return;
                        default:
                            myPropertyInfo.SetValue(Object, Value);
                            return;
                    }
                }
                else
                {



                    var type =  GetTypeCodeO(propertyType);
                    switch (type)
                    {
                        case TypeCode.Int32:
                            myPropertyInfo.SetValue(Object, int.Parse(Value.ToString()));
                            return;
                        case TypeCode.Double:
                            myPropertyInfo.SetValue(Object, double.Parse(Value.ToString()));
                            return;
                        case TypeCode.DateTime:
                            myPropertyInfo.SetValue(Object, DateTime.Parse(Value.ToString()));
                            return;
                        case TypeCode.Int64:
                            myPropertyInfo.SetValue(Object, long.Parse(Value.ToString()));
                            return;
                        case TypeCode.Single:
                            myPropertyInfo.SetValue(Object, float.Parse(Value.ToString()));
                            return;
                        case TypeCode.String:
                            myPropertyInfo.SetValue(Object, (Value.ToString()));
                            return;
                        case TypeCode.Boolean:
                            myPropertyInfo.SetValue(Object, (Value.ToString() == "True"));
                            return;
                        case TypeCode.Object:
                            myPropertyInfo.SetValue(Object, Value);
                            return;
                        default:
                            myPropertyInfo.SetValue(Object, Value);
                            return;
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }
        public static TypeCode GetTypeCodeO(  Type type)
        {
            if (type == typeof(Enum))
                return TypeCode.Int32;
            return Type.GetTypeCode(type);
        }
    }
}
