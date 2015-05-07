using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mappy.Configuration
{
    public interface IConfigurator
    {
        Type EntityType { get; }
    }
}
