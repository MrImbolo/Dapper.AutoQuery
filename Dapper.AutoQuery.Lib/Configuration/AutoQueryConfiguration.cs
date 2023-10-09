using Dapper.AutoQuery.Lib.Queries;
using Dapper.AutoQuery.Lib.SqlDependent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace Dapper.AutoQuery.Lib.Configuration
{
    public class AutoQueryConfiguration
    {
        /// <summary>
        /// Query generation peroperties dependent on specific SQL implementation (T-SQL, MySQL, POSTGRESQL etc.).
        /// <br /> Applyed by specific version of library (e.g. Dapper.AutoQuery.SQLServer) automatically, bu can be configured additionally.
        /// </summary>
        public SQLDependentConfiguration SQLDependentConfig { get; internal set; } = new();


        /// <summary>
        /// Type of attribute preventing property it is applyed to from being used by mapper. Default value is <see cref="NotMappedAttribute"/>
        /// </summary>
        public Type NotMappedType { get; internal set; } = typeof(NotMappedAttribute);

        /// <summary>
        /// Type attribute marking property as key. Default value is <see cref="KeyAttribute"/>
        /// </summary>
        public Type KeyAttributeType { get; internal set; } = typeof(KeyAttribute);

        /// <summary>
        /// May be any but must contain public property "Name". Default value is <see cref="ColumnAttribute"/>
        /// </summary>
        public Type ColumnAttributeType { get; internal set; } = typeof(ColumnAttribute);

        /// <summary>
        /// May be any but must contain public property "Name". Default value is <see cref="TableAttribute"/>
        /// </summary>
        public Type TableAttributeType { get; internal set; } = typeof(TableAttribute);

        public Action<string> LogAction { get; set; } = null!;
        public IQueryLibrary QueryLibrary { get; internal set; }

        public void ThrowIfNotConfigured()
        {
            if (SQLDependentConfig.VarPrefix is null) throw new NoNullAllowedException(nameof(SQLDependentConfig.VarPrefix));

            if (NotMappedType is null) throw new NoNullAllowedException(nameof(NotMappedType));

            if (KeyAttributeType is null) throw new NoNullAllowedException(nameof(KeyAttributeType));

            if (SQLDependentConfig.EscLChar == default)
                throw new NoNullAllowedException(nameof(SQLDependentConfig.EscLChar));

            if (SQLDependentConfig.EscRChar == default)
                throw new NoNullAllowedException(nameof(SQLDependentConfig.EscRChar));

            if (ColumnAttributeType is null) throw new NoNullAllowedException(nameof(ColumnAttributeType));

            if (TableAttributeType is null) throw new NoNullAllowedException(nameof(TableAttributeType));

            if (SQLDependentConfig.FieldLayoutPolicy == default) throw new ArgumentException(nameof(FieldLayoutPolicy));
        }
    }
}