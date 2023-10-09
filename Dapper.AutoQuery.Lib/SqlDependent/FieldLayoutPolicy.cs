using System.Runtime.CompilerServices;

namespace Dapper.AutoQuery.Lib.SqlDependent
{
    public readonly struct FieldLayoutPolicy
    {
        public string Separator { get; }
        public string? Prefix { get; }
        public string? Postfix { get; }

        private FieldLayoutPolicy(string separator, string? prefix = null, string? postfix = null)
        {
            Separator = separator;
            Prefix = prefix;
            Postfix = postfix;
        }
        /// <summary>
        /// All argiments are on the same line and separated by comma
        /// </summary>
        public static FieldLayoutPolicy SameLine => new(",");
        /// <summary>
        /// Each argument appears on a new line ending with comma (except last)
        /// </summary>
        public static FieldLayoutPolicy EndCommaMultiline => new(",", null, Environment.NewLine);
        public static FieldLayoutPolicy EndCommaMultilineOneTab => new(",", null, Environment.NewLine + "    ");

        /// <summary>
        /// Each element appears on a new line starting with comma (except first)
        /// </summary>
        public static FieldLayoutPolicy StartCommaMultiline => new(",", Environment.NewLine);

        public static FieldLayoutPolicy StartCommaMultilineOneTab => new(",", Environment.NewLine + "    ");


        public static bool operator ==(FieldLayoutPolicy policy1, FieldLayoutPolicy policy2)
            => policy1.Separator == policy2.Separator && policy1.Prefix == policy2.Prefix && policy1.Postfix == policy2.Postfix;
        public static bool operator !=(FieldLayoutPolicy policy1, FieldLayoutPolicy policy2)
            => policy1.Separator != policy2.Separator || policy1.Prefix != policy2.Prefix || policy1.Postfix != policy2.Postfix;
        public override string ToString()
        {
            return Prefix + Separator + Postfix;
        }

        public override bool Equals(object? obj)
        {
            return obj is FieldLayoutPolicy policy && this == policy;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Prefix, Separator, Postfix);
        }
    }
}