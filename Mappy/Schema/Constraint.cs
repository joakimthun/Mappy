using System.Data.SqlClient;

namespace Mappy.Schema
{
    internal enum ConstraintType
    {
        PrimaryKey,
        ForeignKey
    }

    internal abstract class Constraint
    {
        public abstract ConstraintType ConstraintType { get; }
    }
}
