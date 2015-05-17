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
            var entityCollection = new EntityCollection(_configuration);
            var entityInfo = new EntityInfo(typeof(TEntity));

            while (reader.Read())
            {
                MapEntity(reader, entityInfo, entityCollection);
            }

            return MapEntityCollection<TEntity>(entityCollection, entityInfo);
        }

        private void MapEntity(SqlDataReader reader, EntityInfo entityInfo, EntityCollection entityCollection)
        {
            var entity = _entityFactory.CreateEntity(entityInfo.EntityType);

            var aliasHelper = _aliasHelpers.SingleOrDefault(x => x.EntityType == entityInfo.EntityType);
            var hasValues = false;

            if (aliasHelper != null)
            {
                foreach (var property in entityInfo.SimpleProperties)
                {
                    var columnName = aliasHelper.GetColumnAlias(property.Name);
                    var columnValue = GetColumnValue(reader, columnName, ref hasValues);
                    SetPropertyValue(property.PropertyInfo, entity, columnValue);
                }
            }

            // If hasValues has not been set to true the entity has all its properties set to default values and we do not need to add it. E.g. a outer join result.
            if (hasValues)
            {
                foreach (var relationship in entityInfo.OneToManyRelationships)
                {
                    MapEntity(reader, relationship, entityCollection);
                }

                foreach (var relationship in entityInfo.OneToOneRelationships)
                {
                    MapEntity(reader, relationship, entityCollection);
                }

                entityCollection.AddDistinctEntity(entity);
            }
        }

        private object GetColumnValue(SqlDataReader reader, string columnName, ref bool hasValue)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var column = reader.GetName(i);
                if (column == columnName)
                {
                    if (reader.IsDBNull(i))
                        return null;

                    var columnType = reader.GetFieldType(i);
                    var value = GetColumnValue(reader, columnType, i);
                    hasValue = true;
                    return value;
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

            if (entityInfo.OneToOneRelationships.Any())
            {
                MapOneToOneRelationships<TEntity>(entities, entityCollection, entityInfo);
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

        private void MapOneToOneRelationships<TEntity>(List<TEntity> entities, EntityCollection entityCollection, EntityInfo entityInfo) where TEntity : new()
        {
            var entityType = typeof(TEntity);

            foreach (var entity in entities)
            {
                foreach (var relationship in entityInfo.OneToOneRelationships)
                {
                    // The following lines of code is used to determine in what table the foreign key column lives
                    var thisForeignKey = _configuration.Schema.Constraints.OfType<ForeignKey>().SingleOrDefault(fk => fk.PkTable.EntityType == entityType && fk.FkTable.EntityType == relationship.EntityType);

                    var otherForeignKey = _configuration.Schema.Constraints.OfType<ForeignKey>().SingleOrDefault(fk => fk.FkTable.EntityType == entityType && fk.PkTable.EntityType == relationship.EntityType);

                    var targetEntityType = thisForeignKey != null ? thisForeignKey.FkTable.EntityType : otherForeignKey.PkTable.EntityType;

                    var relationshipEntities = entityCollection.GetEntities(targetEntityType);

                    var childEntity = GetChildEntity(entity, thisForeignKey ?? otherForeignKey, relationshipEntities);

                    SetPropertyValue(relationship.ParentPropertyInfo, entity, childEntity);
                }
            }
        }

        private IEnumerable<object> GetChildEntities(object parent, ForeignKey foreignKey, IEnumerable<object> children)
        {
            var parentKeyProperty = foreignKey.PkTable.EntityType.GetProperty(foreignKey.PkColumn.Name);
            var childKeyProperty = foreignKey.FkTable.EntityType.GetProperty(foreignKey.FkColumn.Name);

            return children.Where(x => parentKeyProperty.AreValuesEqual(childKeyProperty, parent, x));
        }

        private object GetChildEntity(object parent, ForeignKey foreignKey, IEnumerable<object> children)
        {
            var parentKeyProperty = foreignKey.PkTable.EntityType.GetProperty(foreignKey.PkColumn.Name);
            var childKeyProperty = foreignKey.FkTable.EntityType.GetProperty(foreignKey.FkColumn.Name);

            // TODO: Fix the duplicate values
            var matchingChildren = children.Where(x => parentKeyProperty.AreValuesEqual(childKeyProperty, parent, x));

            if (matchingChildren.Count() != 1)
            {
                var childType = foreignKey.PkTable.EntityType == parent.GetType() ? foreignKey.FkTable.EntityType : foreignKey.PkTable.EntityType;
                throw new MappyException("Expected one '{0}' to match the '{1}' found '{2}' matching entities", childType.FullName, parent.GetType().FullName, matchingChildren.Count());
            }
            
            return matchingChildren.Single();
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
