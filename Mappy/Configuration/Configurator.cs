using Mappy.Exceptions;
using Mappy.Helpers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mappy.Configuration
{
    public class Configurator<TEntity> : IConfigurator where TEntity : new() 
    {
        private readonly List<string> _primaryKeys;
        private readonly List<HasMany> _hasMany;

        public Configurator(Type type)
        {
            EntityType = type;

            _primaryKeys = new List<string>();
            _hasMany = new List<HasMany>();
        }

        public Type EntityType { get; private set; }

        public Configurator<TEntity> HasPrimaryKey<TProperty>(Expression<Func<TEntity, TProperty>> expression)
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

        public ForeignKeyConfigurator<TOtherEntity, TEntity> HasMany<TOtherEntity>(Expression<Func<TEntity, ICollection<TOtherEntity>>> expression)
        {
            try
            {
                return new ForeignKeyConfigurator<TOtherEntity, TEntity>(this, ExpressionHelper.GetPropertyName(expression));
            }
            catch (ArgumentException)
            {
                throw new MappyException("An invalid has many configuration was added for the entity '{0}'", typeof(TEntity).Name);
            }
        }

        internal void AddHasMany(HasMany hasMany)
        {
            _hasMany.Add(hasMany);
        }
    }
}
