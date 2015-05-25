using Mappy.Exceptions;
using Mappy.Helpers;
using System;
using System.Linq.Expressions;

namespace Mappy.Configuration
{
    public class ForeignKeyConfigurator<TEntity, TOtherEntity> where TOtherEntity : new()
    {
        private readonly Configurator<TOtherEntity> _parent;
        private readonly string _parentPropertyName;

        public ForeignKeyConfigurator(Configurator<TOtherEntity> configurator, string parentPropertyName)
        {
            _parent = configurator;
            _parentPropertyName = parentPropertyName;
        }

        public Configurator<TOtherEntity> HasForeignKey<TProperty>(Expression<Func<TEntity, TProperty>> expression)
        {
            try
            {
                var hasMany = new HasMany
                {
                    Property = _parentPropertyName,
                    Type = typeof(TEntity),
                    ForeignKey = ExpressionHelper.GetPropertyName(expression)
                };

                _parent.AddHasMany(hasMany);

                return _parent;
            }
            catch (ArgumentException)
            {
                throw new MappyException($"An invalid has foreign key was added for the entity '{typeof(TEntity).Name}'");
            }
        }
    }
}
