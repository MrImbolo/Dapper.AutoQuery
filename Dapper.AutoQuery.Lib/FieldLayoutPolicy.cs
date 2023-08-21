namespace Dapper.AutoQuery.Lib
{
    public struct FieldLayoutPolicy
    {
        public string Separator { get; }

        private FieldLayoutPolicy(string separator)
        {
            Separator = separator;
        }
        /// <summary>
        /// All argiments are on the same line and separated by comma
        /// </summary>
        public static FieldLayoutPolicy SameLine => new FieldLayoutPolicy(",");
        /// <summary>
        /// Each argument appears on a new line ending with comma (except last)
        /// </summary>
        public static FieldLayoutPolicy EndCommaMultiline => new FieldLayoutPolicy("," + Environment.NewLine);
        public static FieldLayoutPolicy EndCommaMultilineOneTab => new FieldLayoutPolicy("," + Environment.NewLine + "    ");

        /// <summary>
        /// Each element appears on a new line starting with comma (except first)
        /// </summary>
        public static FieldLayoutPolicy StartCommaMultiline => new FieldLayoutPolicy(Environment.NewLine + ",");

        public static FieldLayoutPolicy StartCommaMultilineOneTab => new FieldLayoutPolicy(Environment.NewLine + "    ,");


        public static bool operator ==(FieldLayoutPolicy policy1, FieldLayoutPolicy policy2) 
            => policy1.Separator == policy2.Separator;
        public static bool operator !=(FieldLayoutPolicy policy1, FieldLayoutPolicy policy2) 
            => policy1.Separator != policy2.Separator;
        public override string ToString()
        {
            return Separator;
        }
    }
}