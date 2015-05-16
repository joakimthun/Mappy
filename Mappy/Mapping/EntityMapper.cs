using Mappy.Configuration;
using Mappy.Exceptions;
using Mappy.Extensions;
using Mappy.Queries;
using Mappy.Reflection;
using Mappy.Schema;
using System;
using System.Collections;
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
            var entityCollection = new EntityCollection();
            var entityInfo = new EntityInfo(typeof(TEntity));

            while (reader.Read())
            {
                MapEntity(reader, entityInfo, entityCollection);
            }

            return MapEntityCollection<TEntity>(entityCollection, entityInfo);
        }

        private object MapEntity(SqlDataReader reader, EntityInfo entityInfo, EntityCollection entityCollection)
        {
            var entity = _entityFactory.CreateEntity(entityInfo.EntityType);

            var aliasHelper = _aliasHelpers.SingleOrDefault(x => x.EntityType == entityInfo.EntityType);
            if (aliasHelper != null)
            {
                foreach (var property in entityInfo.SimpleProperties)
                {
                    var columnName = aliasHelper.GetColumnAlias(property.Name);
                    var columnValue = GetColumnValue(reader, columnName);
                    SetPropertyValue(property.PropertyInfo, entity, columnValue);
                }
            }

            foreach (var relationship in entityInfo.OneToManyRelationships)
            {
                MapEntity(reader, relationship, entityCollection);
            }

            foreach (var relationship in entityInfo.OneToOneRelationships)
            {
                MapEntity(reader, relationship, entityCollection);
            }

            entityCollection.AddEntity(entity);

            return entity;
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

        private IEnumerable<TEntity> MapEntityCollection<TEntity>(EntityCollection entityCollection, EntityInfo entityInfo) where TEntity : new()
        {
            var entities = new List<TEntity>();

            entities.AddRange(GetDistinctEntities<TEntity>(entityCollection));

            if (entityInfo.OneToManyRelationships.Any())
            {
                MapOneToManyRelationships<TEntity>(entities, entityCollection, entityInfo);
            }

            return entities;
        }

        private void MapOneToManyRelationships<TEntity>(List<TEntity> entities, EntityCollection entityCollection, EntityInfo entityInfo) where TEntity : new()
        {
            var entityType = typeof(TEntity);

            foreach (var entity in entities)
            {
                foreach (var relationship in entityInfo.OneToManyRelationships)
                {
                    var foreignKey = _configuration.Schema.Constraints.OfType<ForeignKey>().Single(fk => fk.PkTable.EntityType == entityType && fk.FkTable.EntityType == relationship.EntityType);
                    var relationshipEntities = entityCollection.GetEntities(foreignKey.FkTable.EntityType);

                    var childEntities = GetChildEntities(entity, foreignKey, relationshipEntities);
                    if (childEntities.Any())
                    {
                        SetCollectionPropertyValue(relationship.ParentPropertyInfo, entity, childEntities, foreignKey.FkTable.EntityType);
                    }
                }
            }
        }

        private IEnumerable<object> GetChildEntities(object parent, ForeignKey foreignKey, IEnumerable<object> children)
        {
            var parentKeyProperty = foreignKey.PkTable.EntityType.GetProperty(foreignKey.PkColumn.Name);
            var childKeyProperty = foreignKey.FkTable.EntityType.GetProperty(foreignKey.FkColumn.Name);

            return children.Where(x => parentKeyProperty.AreValuesEqual(childKeyProperty, parent, x));
        }

        private IEnumerable<TEntity> GetDistinctEntities<TEntity>(EntityCollection entityCollection) where TEntity : new()
        {
            var entityType = typeof(TEntity);
            var entities = entityCollection.GetEntities(entityType) ?? new List<object>();
            var distinctEntities = new List<TEntity>();

            var primaryKey = _configuration.Schema.Constraints.OfType<PrimaryKey>().Single(pk => pk.Table.EntityType == entityType);
            var primaryKeyProperty = entityType.GetProperty(primaryKey.Column.Name);

            foreach (var entity in entities)
            {
                if (!distinctEntities.Any(e => primaryKeyProperty.AreValuesEqual(entity, e)))
                {
                    distinctEntities.Add((TEntity)entity);
                }
            }

            return distinctEntities;
        }

        private void SetPropertyValue(PropertyInfo property, object entity, object value)
        {
            property.SetValue(entity, value);
        }

        private void SetCollectionPropertyValue(PropertyInfo property, object entity, IEnumerable<object> values, Type collectionType)
        {
            if (property.GetValue(entity) == null)
            {
                var type = typeof(List<>).MakeGenericType(collectionType);
                var collection = Activator.CreateInstance(type);
                property.SetValue(entity, collection);
            }

            var propertyValue = property.GetValue(entity);

            foreach (var value in values)
            {
                ((IList)propertyValue).Add(value);
            }
        }
    }
}
