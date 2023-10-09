using Dapper.AutoQuery.Lib.Core;
using Dapper.AutoQuery.Lib.SqlDependent;

namespace Dapper.AutoQuery
{
    public static class AutoQueryBuilderExtensions
    {
        public static IAutoQueryConfigurationBuilder SetUpSQLServer(this IAutoQueryConfigurationBuilder autoQueryConfigurationBuilder)
        {
            autoQueryConfigurationBuilder
                .ConfigureSQLDependentConfiguration(config =>
                {
                    config.VarPrefix = "@";
                    config.QuoteSign = "\'";
                    config.EscLChar = "[";
                    config.EscRChar = "]";
                    config.FieldLayoutPolicy = FieldLayoutPolicy.StartCommaMultilineOneTab;
                })
                .SetUpQueryLibrary(new SQLServerQueryLibrary());

            return autoQueryConfigurationBuilder;
        }
    }
}