using Dapper.AutoQuery.Lib.Configuration;
using Dapper.AutoQuery.Lib.Exceptions;
using Dapper.AutoQuery.Lib.Queries;
using Humanizer;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.AutoQuery.Lib.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CustomFieldAttribute : Attribute
    {
        public CustomFieldAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    //public enum DAQTokenType
    //{
    //    Table,
    //    Field,
    //    Eq,
    //    Gt,
    //    Lt,
    //    Gte,
    //    Lte,
    //    IN
    //}


    public readonly struct DAQToken
    {

        public DAQToken(string sqlName, string objectName, bool isKey = false)
        {
            SqlName = sqlName;
            ObjectName = objectName;
            IsKey = isKey;
        }
        public bool IsCaseSensitive { get; } = true;
        public bool IsKey { get; } = false;

        public string SqlName { get; }
        public string ObjectName { get; }

        public static implicit operator DAQToken(string token) => new DAQToken(token, token);
        public static implicit operator string(DAQToken token) => token.SqlName;

        public override string ToString() => SqlName;

    }

    public interface IAutoQueryConfigurationBuilder
    {
        IAutoQueryConfigurationBuilder SetNotMappedAttributeType(Type notMappedType);
        IAutoQueryConfigurationBuilder SetKeyAttributeType(Type keyAttributeType);
        IAutoQueryConfigurationBuilder SetColumnAttributeType(Type columnAttributeType);
        IAutoQueryConfigurationBuilder SetTableAttributeType(Type tableAttributeType);

        IAutoQueryConfigurationBuilder SetUpLogAction(Action<string> actions);

        AutoQueryConfiguration Build();
        IAutoQueryConfigurationBuilder ConfigureSQLDependentConfiguration(Action<SQLDependentConfiguration> config);
        IAutoQueryConfigurationBuilder SetUpQueryLibrary(IQueryLibrary lib);
    }

    public static class Convensions
    {
        public static string KeyFieldDefaultName { get; set; } = "Id";
    }

    public static class EntityManager
    {
        public static void WarmUp<T>() where T : class => EntityManager<T>.GetInstance();
        public static EntityManager<T> GetInstance<T>()
            where T : class
        {
            return EntityManager<T>.GetInstance();
        }
    }
    public class EntityManager<T>
        where T : class
    {
        public EntityManager(AutoQueryConfiguration config)
        {
            Config = config;
            Members = new EntityMembers<T>(AutoQueryGenerator.Configuration);
            RawTableName = GetTableName();
            TableName = Config.SQLDependentConfig.EscLChar +  GetTableName() + Config.SQLDependentConfig.EscRChar;
        }

        public EntityManager(Action<AutoQueryConfiguration> configure)
        {
            Config = new AutoQueryConfiguration();
            configure(AutoQueryGenerator.Configuration);
            Members = new EntityMembers<T>(AutoQueryGenerator.Configuration);
            RawTableName = GetTableName();
            TableName = Config.SQLDependentConfig.EscLChar +  GetTableName() + Config.SQLDependentConfig.EscRChar;
        }

        private static EntityManager<T> _instance = null!;
        public static EntityManager<T> GetInstance()
        {
            return _instance ??= new EntityManager<T>(AutoQueryGenerator.Configuration);
        }

        AutoQueryConfiguration Config { get; init; }
        public EntityMembers<T> Members { get; init; }
        public string RawTableName { get; }
        public string TableName { get; }

        public string GetTableName()
        {
            var attr = typeof(T).GetCustomAttribute(Config.TableAttributeType);
            if (attr is null)
            {
                return typeof(T).Name.Pluralize();
            }

            var attrPropNameValueOrDefault = (string?)Config.TableAttributeType.GetProperty("Name")?.GetValue(attr);
            var name = attrPropNameValueOrDefault ?? typeof(T).Name.Pluralize();

            return name;
        }
    }
}