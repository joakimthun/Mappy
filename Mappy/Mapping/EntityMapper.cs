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
                result.Add((TEntity)MapEntity(reader, new EntityInfo(typeof(TEntity))));
            }

            return result;
        }

        private object MapEntity(SqlDataReader reader, EntityInfo entityInfo)
        {
            var entity = _entityFactory.CreateEntity(entityInfo.EntityType);

            foreach (var property in entityInfo.SimpleProperties)
            {
                var aliasHelper = _aliasHelpers.Single(x => x.EntityType == entityInfo.EntityType);
                var columnName = aliasHelper.GetColumnAlias(property.Name);

                var columnValue = GetColumnValue(reader, columnName);
                SetPropertyValue(property.PropertyInfo, entity, columnValue);
            }

            foreach (var relationship in entityInfo.OneToManyRelationships)
            {
                MapEntity(reader, relationship);
            }

            return entity;
        }

        private void SetPropertyValue(PropertyInfo property, object entity, object value)
        {
            property.SetValue(entity, value);
        }

        private object GetColumnValue(SqlDataReader reader, string columnName)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var column = reader.GetName(i);
                if (column == columnName)
                {
                    if (reader.IsDBNull(i))
                        return null;

                    var columnType = reader.GetFieldType(i);
                    return GetColumnValue(reader, columnType, i);
                }
            }

            return null;
        }

        private object GetColumnValue(SqlDataReader reader, Type columnType, int index)
        {
            return MethodInvoker.InvokeGenericMethod(reader, "GetFieldValue", columnType, index);
        }
    }
}
