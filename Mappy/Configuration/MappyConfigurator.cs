using Mappy.Exceptions;
using Mappy.Helpers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mappy.Configuration
{
    public class MappyConfigurator<TEntity> : IMappyConfigurator where TEntity : new() 
    {
        private readonly List<string> _primaryKeys;

        public MappyConfigurator(Type type)
        {
            EntityType = type;

            _primaryKeys = new List<string>();
        }

        public Type EntityType { get; private set; }

        public MappyConfigurator<TEntity> HasPrimaryKey<TProperty>(Expression<Func<TEntity, TProperty>> expression)
        {
            try
            {
                _primaryKeys.Add(ExpressionHelper.GetPropertyName(expression));
            }
            catch (ArgumentException)
            {
                throw new MappyException("An invalid primary key configuration was added for the entity '{0}'", typeof(TEntity).Name);
            }

            return this;
        }
    }
}
