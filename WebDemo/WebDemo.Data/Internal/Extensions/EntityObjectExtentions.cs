using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace WebDemo.Data.Internal.Extensions
{
    public static class EntityObjectExtentions
    {
        #region data to entity hepers

        public static T ToObject<T>(this DataTable dataTable) where T : new()
        {
            if (dataTable == null || dataTable.Rows.Count <= 0)
            {
                return default;
            }
            return Materializer<T>(dataTable.Rows[0]);
        }

        public static ICollection<T> ToObjectCollection<T>(this DataTable dataTable) where T : new()
        {
            ICollection<T> data = new List<T>();

            if (dataTable == null || dataTable.Rows.Count <= 0)
            {
                return data;
            }

            foreach (DataRow dataRow in dataTable.Rows)
            {
                data.Add(Materializer<T>(dataRow));
            }

            return data;
        }

        public static T ToObject<T>(this IDataReader dataReader) where T : new()
        {
            if (dataReader == null || !dataReader.Read())
            {
                return default;
            }

            return Materializer<T>(dataReader);
        }

        public static ICollection<T> ToObjectCollection<T>(this IDataReader dataReader) where T : new()
        {
            ICollection<T> data = new List<T>();

            if (dataReader != null)
            {
                while (dataReader.Read())
                {
                    data.Add(Materializer<T>(dataReader));
                }
            }

            return data;
        }

        static T Materializer<T>(DataRow dataRow) where T : new()
        {
            // Create a new type of the entity
            Type t = typeof(T);
            T returnObject = new T();

            PropertyInfo[] perperties = t.GetProperties();
            foreach (PropertyInfo prop in perperties)
            {
                if (!prop.CanWrite ||
                    !dataRow.Table.Columns.Contains(prop.Name) ||
                    dataRow.IsNull(prop.Name))
                {
                    continue;
                }

                object val = dataRow[prop.Name];
                SetPropertyValue(returnObject, prop, val);
            }

            return returnObject;
        }

        static T Materializer<T>(IDataReader dataReader) where T : new()
        {
            // Create a new type of the entity
            Type t = typeof(T);
            T returnObject = new T();

            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                PropertyInfo prop = t.GetProperty(dataReader.GetName(i), BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
                if (prop == null)
                {
                    continue;
                }
                if (!prop.CanWrite)
                {
                    continue;
                }
                var val = dataReader.GetValue(i);
                SetPropertyValue(returnObject, prop, val);

            }

            return returnObject;
        }

        static void SetPropertyValue(object obj, PropertyInfo property, object value)
        {
            if (value == DBNull.Value || value == null)
            {
                return;
            }

            var propertyType = property.PropertyType;
            var propertyTypeInfo = propertyType.GetTypeInfo();

            // is this a Nullable<> type
            if (Nullable.GetUnderlyingType(propertyType) != null)
            {
                // Convert the db type into the T we have in our Nullable<T> type
                value = Convert.ChangeType(value, Nullable.GetUnderlyingType(property.PropertyType));
            }
            else if (propertyTypeInfo.IsEnum)
            {
                value = Enum.Parse(property.PropertyType, value.ToString());
            }
            else
            {
                // Convert the db type into the type of the property in our entity
                value = Convert.ChangeType(value, property.PropertyType);
            }

            // Set the value of the property with the value from the db
            property.SetValue(obj, value, null);
        }

        static bool HasField(this IDataReader dataReader, string columnName)
        {
            int count = dataReader.FieldCount;
            for (int i = 0; i < count; i++)
            {
                if (dataReader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
