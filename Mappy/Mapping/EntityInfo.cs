using Mappy.Configuration;
using Mappy.Extensions;
using System;
using System.Collections.Generic;

namespace Mappy.Mapping
{
    internal class EntityInfo
    {
        public EntityInfo(Type type)
        {
            EntityType = type;
            SimpleProperties = new List<Property>();
            OneToOneRelationships = new List<EntityInfo>();
            OneToManyRelationships = new List<EntityInfo>();

            GetEntityInfo();
        }

        public Type EntityType { get; set; }

        public ICollection<Property> SimpleProperties { get; set; }

        public ICollection<EntityInfo> OneToOneRelationships { get; set; }

        public ICollection<EntityInfo> OneToManyRelationships { get; set; }

        private void GetEntityInfo()
        {
            var properties = EntityType.GetPublicInstanceProperties();

            foreach (var property in properties)
            {
                if (SupportedTypes.Contains(property.PropertyType))
                {
                    SimpleProperties.Add(new Property { PropertyInfo = property });
                }
                else if (property.GetUnderlyingPropertyType().IsMappyEntity())
                {
                    if (property.IsICollection())
                    {
                        OneToManyRelationships.Add(new EntityInfo(property.GetUnderlyingPropertyType()));
                    }
                    else
                    {
                        OneToOneRelationships.Add(new EntityInfo(property.PropertyType));
                    }
                }
            }
        }
    }
}
