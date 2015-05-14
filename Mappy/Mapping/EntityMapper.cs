using Mappy.Configuration;
using Mappy.Exceptions;
using Mappy.Queries;
using Mappy.Reflection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace Mappy.Mapping
{
    internal class EntityMapper
    {
        private readonly MappyConfiguration _configuration;
        private readonly EntityFactory _entityFactory;
        private readonly List<AliasHelper> _aliasHelpers;

        public EntityMapper(MappyConfiguration configuration, List<AliasHelper> aliasHelpers)
        {
            _configuration = configuration;
            _entityFactory = new EntityFactory();
            _aliasHelpers = aliasHelpers;
        }

        public IEnumerable<TEntity> Map<TEntity>(SqlDataReader reader) where TEntity : new()
        {
            var type = typeof(TEntity);
            if (!ContainsTable(type))
                throw new MappyException("The entity type '{0}' has no matching table in the database.", type.Name);

            return MapImpl<TEntity>(reader);
        }

        private bool ContainsTable(Type entityType)
        {
            foreach (var table in _configuration.Schema.Tables)
            {
                if (table.Name == entityType.Name)
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<TEntity> MapImpl<TEntity>(SqlDataReader reader) where TEntity : new()
        {
            var result = new List<TEntity>();

            while (reader.Read())
            {
                result.Add(MapEntity<TEntity>(reader));
            }

            return result;
        }

        private TEntity MapEntity<TEntity>(SqlDataReader reader) where TEntity : new()
        {
            var entity = _entityFactory.CreateEntity<TEntity>();
            var aliasHelper = _aliasHelpers.Single(x => x.EntityType == typeof(TEntity));

            for (var column = 0; column < reader.FieldCount; column++)
            {
                var columnName = reader.GetName(column);

                if (!aliasHelper.ColumnBelongsToEntity(columnName))
                    continue;

                var columnType = reader.GetFieldType(column);
                var value = GetColumnValue(reader, columnType, column);

                var propertyName = aliasHelper.GetPropertyName(columnName);

                TrySetPropertyValue(entity, value, propertyName);
            }

            return entity;
        }

        private object GetColumnValue(SqlDataReader reader, Type columnType, int column)
        {
            return MethodInvoker.InvokeGenericMethod(reader, "GetFieldValue", columnType, column);
        }

        private void TrySetPropertyValue(object obj, object value, string propertyName)
        {
            var type = obj.GetType();
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                return;

            property.SetValue(obj, value);
        }
    }
}
