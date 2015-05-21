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
        private readonly bool _lazyLoading;

        public EntityMapper(MappyConfiguration configuration, List<AliasHelper> aliasHelpers, bool lazyLoading)
        {
            _configuration = configuration;
            _entityFactory = new EntityFactory(configuration, lazyLoading);
            _aliasHelpers = aliasHelpers;
            _lazyLoading = lazyLoading;
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

            return MapEntitiesFromCollection(entityCollection, entityInfo).Cast<TEntity>();
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

        private IEnumerable<object> MapEntitiesFromCollection(EntityCollection entityCollection, EntityInfo entityInfo)
        {
            var entities = new List<object>();

            foreach (var entity in entityCollection.GetEntities(entityInfo.EntityType))
            {
                entities.Add(MapEntityFromCollection(entity, entityCollection, entityInfo));
            }

            return entities;
        }

        private object MapEntityFromCollection(object entity, EntityCollection entityCollection, EntityInfo entityInfo)
        {
            if (entity == null)
                return null;

            if (entityInfo.OneToManyRelationships.Any())
            {
                MapOneToManyRelationships(entity, entityCollection, entityInfo);
            }

            if (entityInfo.OneToOneRelationships.Any())
            {
                MapOneToOneRelationships(entity, entityCollection, entityInfo);
            }

            return entity;
        }

        private void MapOneToManyRelationships(object entity, EntityCollection entityCollection, EntityInfo entityInfo)
        {
            foreach (var relationship in entityInfo.OneToManyRelationships)
            {
                var foreignKey = _configuration.Schema.Constraints.OfType<ForeignKey>().Single(fk => fk.PkTable.EntityType == entityInfo.EntityType && fk.FkTable.EntityType == relationship.EntityType);
                var relationshipEntities = entityCollection.GetEntities(foreignKey.FkTable.EntityType);

                var childEntities = GetChildEntities(entity, foreignKey, relationshipEntities);

                if (childEntities == null)
                    continue;

                foreach (var childEntity in childEntities)
                {
                    MapEntityFromCollection(childEntity, entityCollection, relationship);
                }

                if (childEntities.Any())
                {
                    SetCollectionPropertyValue(relationship.ParentPropertyInfo, entity, childEntities, foreignKey.FkTable.EntityType);
                }
            }
        }

        private void MapOneToOneRelationships(object entity, EntityCollection entityCollection, EntityInfo entityInfo)
        {
            foreach (var relationship in entityInfo.OneToOneRelationships)
            {
                // The following lines of code is used to determine in what table the foreign key column lives
                var thisForeignKey = _configuration.Schema.Constraints.OfType<ForeignKey>().SingleOrDefault(fk => fk.PkTable.EntityType == entityInfo.EntityType && fk.FkTable.EntityType == relationship.EntityType);

                var otherForeignKey = _configuration.Schema.Constraints.OfType<ForeignKey>().SingleOrDefault(fk => fk.FkTable.EntityType == entityInfo.EntityType && fk.PkTable.EntityType == relationship.EntityType);

                var targetEntityType = thisForeignKey != null ? thisForeignKey.FkTable.EntityType : otherForeignKey.PkTable.EntityType;

                var relationshipEntities = entityCollection.GetEntities(targetEntityType);

                var childEntity = GetChildEntity(entity, thisForeignKey ?? otherForeignKey, relationshipEntities);

                MapEntityFromCollection(childEntity, entityCollection, relationship);

                SetPropertyValue(relationship.ParentPropertyInfo, entity, childEntity);
            }
        }

        private IEnumerable<object> GetChildEntities(object parent, ForeignKey foreignKey, IEnumerable<object> children)
        {
            if (children == null)
                return null;

            var parentKeyProperty = foreignKey.PkTable.EntityType.GetProperty(foreignKey.PkColumn.Name);
            var childKeyProperty = foreignKey.FkTable.EntityType.GetProperty(foreignKey.FkColumn.Name);

            return children.Where(x => parentKeyProperty.AreValuesEqual(childKeyProperty, parent, x));
        }

        private object GetChildEntity(object parent, ForeignKey foreignKey, IEnumerable<object> children)
        {
            if (children == null)
                return null;

            var parentKeyProperty = foreignKey.PkTable.EntityType.GetProperty(foreignKey.PkColumn.Name);
            var childKeyProperty = foreignKey.FkTable.EntityType.GetProperty(foreignKey.FkColumn.Name);

            var matchingChildren = children.Where(x => parentKeyProperty.AreValuesEqual(childKeyProperty, parent, x));

            if (matchingChildren.Count() != 1)
            {
                var childType = foreignKey.PkTable.EntityType == parent.GetType() ? foreignKey.FkTable.EntityType : foreignKey.PkTable.EntityType;
                throw new MappyException("Expected one '{0}' to match the '{1}' found '{2}' matching entities", childType.FullName, parent.GetType().FullName, matchingChildren.Count());
            }
            
            return matchingChildren.Single();
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
