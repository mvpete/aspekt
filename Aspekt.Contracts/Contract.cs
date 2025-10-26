namespace Aspekt.Contracts
{
    public static class Contract
    {
        public enum Target { Field, Property }
        public enum Constraint { NotNull, Null }

        public enum Comparison { LessThan, LessThanEqualTo, GreaterThan, GreaterThanEqualTo, EqualTo, NotEqualTo };
    }
}
