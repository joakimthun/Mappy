namespace Mappy.Schema
{
    internal class ForeignKey : Constraint
    {
        public Column References { get; set; }
        public bool IsNullable { get; set; }
    }
}
