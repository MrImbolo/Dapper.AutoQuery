using Dapper.AutoQuery.Lib.SqlDependent;

namespace Dapper.AutoQuery.Lib.Configuration
{
    public class SQLDependentConfiguration
    {
        public string VarPrefix { get; set; } = null!;
        public FieldLayoutPolicy FieldLayoutPolicy { get; set; } = default;

        public string EscLChar { get; set; }
        public string EscRChar { get; set; }
        public string QuoteSign { get; set; }
    }
}