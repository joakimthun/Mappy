using Mappy.Exceptions;
using Mappy.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Mappy.Mapping
{
    internal class EntityFactory
    {
        public EntityFactory()
        {
        }

        public TEntity CreateEntity<TEntity>(SqlDataReader reader) where TEntity : new()
        {
            var entity = Activator.CreateInstance<TEntity>();

            for (var column = 0; column < reader.FieldCount; column++)
            {
                var columnName = reader.GetName(column);
                var columnType = reader.GetFieldType(column);
                var value = GetColumnValue(reader, columnType, column);

                TrySetPropertyValue(entity, value, columnName);
            }

            return entity;
        }

        private void TrySetPropertyValue(object obj, object value, string columnName)
        {
            var type = obj.GetType();
            var property = type.GetProperty(columnName, BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                return;

            property.SetValue(obj, value);
        }

        private object GetColumnValue(SqlDataReader reader, Type columnType, int column)
        {
            return MethodInvoker.InvokeGenericMethod(reader, "GetFieldValue", columnType, column);
        }
    }
}
