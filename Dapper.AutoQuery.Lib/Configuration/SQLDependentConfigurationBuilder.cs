using Dapper.AutoQuery.Lib.Core;
using Dapper.AutoQuery.Lib.Queries;
using System.Data;
using System.Reflection;

namespace Dapper.AutoQuery.Lib.Configuration
{
    public class SQLDependentConfigurationBuilder : IAutoQueryConfigurationBuilder
    {
        public AutoQueryConfiguration Configuration { get; protected set; } = new();
        public SQLDependentConfigurationBuilder()
        {
        }

        IAutoQueryConfigurationBuilder IAutoQueryConfigurationBuilder.SetNotMappedAttributeType(Type notMappedType)
        {
            Configuration.NotMappedType = notMappedType;
            return this;
        }

        IAutoQueryConfigurationBuilder IAutoQueryConfigurationBuilder.SetKeyAttributeType(Type keyAttributeType)
        {
            Configuration.KeyAttributeType = keyAttributeType;
            return this;
        }
        IAutoQueryConfigurationBuilder IAutoQueryConfigurationBuilder.SetTableAttributeType(Type tableAttributeType)
        {
            Configuration.TableAttributeType = tableAttributeType;
            return this;
        }

        private List<Type> GetTypesWithColumnAttribute()
        {
            return Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsClass
                    && x.GetProperties().Any(x => x.GetCustomAttribute(Configuration.ColumnAttributeType, true) is not null))
                .ToList();
        }
        public IAutoQueryConfigurationBuilder SetUpCustomColumnAttributeSupport()
        {
            if (Configuration.ColumnAttributeType is null)
            {
                throw new InvalidOperationException(
                    $"Error setting custom column attributes to Dapper. Column Attributes are not set. Execute {nameof(SetColumnAttributeType)}");
            }

            foreach (var type in GetTypesWithColumnAttribute())
            {
                SqlMapper.SetTypeMap(type, new CustomPropertyTypeMap(type, (type, columnName) =>
                {
                    return type
                        .GetProperties()
                        .FirstOrDefault(prop => //true
                        {
                            var attr = prop.GetCustomAttribute(Configuration.ColumnAttributeType, false);
                            var columnAttributeNamePropertyValue = (string?)Configuration.ColumnAttributeType
                                .GetProperty("Name")?.GetValue(attr);
                            return columnAttributeNamePropertyValue == columnName;
                        });
                }));
            }
            return this;
        }
        public IAutoQueryConfigurationBuilder SetUpLogAction(Action<string> action)
        {
            Configuration.LogAction = action;
            return this;
        }

        public IAutoQueryConfigurationBuilder SetColumnAttributeType(Type columnAttributeType)
        {
            Configuration.ColumnAttributeType = columnAttributeType;
            return this;
        }

        public AutoQueryConfiguration Build()
        {
            Configuration.ThrowIfNotConfigured();
            Core.AutoQueryGenerator.Configuration = Configuration;
            return Configuration;
        }

        public IAutoQueryConfigurationBuilder ConfigureSQLDependentConfiguration(Action<SQLDependentConfiguration> config)
        {
            config(Configuration.SQLDependentConfig);
            return this;
        }

        public IAutoQueryConfigurationBuilder SetUpQueryLibrary(IQueryLibrary lib)
        {
            ArgumentNullException.ThrowIfNull(lib, nameof(lib));
            Configuration.QueryLibrary = lib;
            return this;
        }
    }
}