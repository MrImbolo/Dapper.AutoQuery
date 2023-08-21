using Dapper.AutoQuery.Lib.Exceptions;
using Dapper.AutoQuery.Lib.Misc;
using Humanizer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Dapper.AutoQuery.Lib
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

    public enum DAQTokenType
    {
        Table,
        Field,
        Eq,
        Gt,
        Lt,
        Gte,
        Lte,
        IN
    }

    
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

    public interface IEntityManagerConfigurationBuilder
    {
        IEntityManagerConfigurationBuilder SetVarPrefix(string varPrefix);
        IEntityManagerConfigurationBuilder SetNotMappedAttributeType(Type notMappedType);
        IEntityManagerConfigurationBuilder SetKeyAttributeType(Type keyAttributeType);
        IEntityManagerConfigurationBuilder SetColumnAttributeType(Type columnAttributeType);
        IEntityManagerConfigurationBuilder SetTableAttributeType(Type tableAttributeType);
        IEntityManagerConfigurationBuilder SetLayoutPolicy(FieldLayoutPolicy fieldLayoutPolicy);

        DAQConfiguration SetGlobally();
    }

    public static class DAQDefaults
    {
        static DAQDefaults()
        {
            Configuration = new DAQConfiguration();
        }
        public static DAQConfiguration Configuration { get; private set; } 
        public static IEntityManagerConfigurationBuilder CreateBuilder() => Configuration;

    }
    public class DAQConfiguration : IEntityManagerConfigurationBuilder
    {
        public string VarPrefix { get; private set; } = "@";
        public Type NotMappedType { get; private set; } = typeof(NotMappedAttribute);
        public Type KeyAttributeType { get; private set; } = typeof(KeyAttribute);

        /// <summary>
        /// May be any but must contain public property "Name"
        /// </summary>
        public Type ColumnAttributeType { get; private set; } = typeof(ColumnAttribute);

        /// <summary>
        /// May be any but must contain public property "Name"
        /// </summary>
        public Type TableAttributeType { get; private set; } = typeof(TableAttribute);
        public FieldLayoutPolicy FieldLayoutPolicy { get; private set; } = FieldLayoutPolicy.SameLine;

        IEntityManagerConfigurationBuilder IEntityManagerConfigurationBuilder.SetVarPrefix(string varPrefix)
        {
            VarPrefix = varPrefix;
            return this;
        }

        IEntityManagerConfigurationBuilder IEntityManagerConfigurationBuilder.SetNotMappedAttributeType(Type notMappedType)
        {
            NotMappedType = notMappedType;
            return this;
        }

        IEntityManagerConfigurationBuilder IEntityManagerConfigurationBuilder.SetKeyAttributeType(Type keyAttributeType)
        {
            KeyAttributeType = keyAttributeType;
            return this;
        }
        IEntityManagerConfigurationBuilder IEntityManagerConfigurationBuilder.SetTableAttributeType(Type tableAttributeType)
        {
            TableAttributeType = tableAttributeType;
            return this;
        }

        IEntityManagerConfigurationBuilder IEntityManagerConfigurationBuilder.SetLayoutPolicy(FieldLayoutPolicy fieldLayoutPolicy)
        {
            FieldLayoutPolicy = fieldLayoutPolicy;
            return this;
        }
        public IEntityManagerConfigurationBuilder SetColumnAttributeType(Type columnAttributeType)
        {
            ColumnAttributeType = columnAttributeType;
            return this;
        }

        public DAQConfiguration SetGlobally()
        {
            return this;
        }

    }

    public static class Convensions
    {
        public static string KeyFieldDefaultName { get; set; } = "Id";
    }

    public static class EntityManager
    {
        public static EntityManager<T> GetInstance<T>() 
            where T : class
        {
            return EntityManager<T>.GetInstance();
        }
    }
    public class EntityManager<T>
        where T : class
    {
        public EntityManager(DAQConfiguration config)
        {
            Config = config;
            Members = new EntityMembers<T>(Config);
            TableName = GetTableName();
        }

        public EntityManager(Action<IEntityManagerConfigurationBuilder> configure)
        {
            Config = new DAQConfiguration();
            configure(Config);
            Members = new EntityMembers<T>(Config);
            TableName = GetTableName();
        }

        private static EntityManager<T> _instance = null!;
        public static EntityManager<T> GetInstance()
        {
            return _instance ??= new EntityManager<T>(DAQDefaults.Configuration);
        }

        DAQConfiguration Config { get; init; }
        public EntityMembers<T> Members { get; init; }

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

    public class EntityMembers<T>
        where T : class
    {
        public DAQConfiguration Config { get; private set; }

        public EntityMembers(DAQConfiguration entityManagerConfiguration)
        {
            ArgumentNullException.ThrowIfNull(entityManagerConfiguration, nameof(Config));

            Config = entityManagerConfiguration;

            All = GetTypeTableFields(typeof(T)).ToDictionary(x => x.Key, y => y.Value);

            AsFullFieldSet = All.Values.Stringify(Separator);
            AsNoKeyFieldSet = All.Values.Where(x => !x.IsKey).Stringify(Separator);
            AsFullArgSet = All.Values.Stringify(Separator, x => Config.VarPrefix + x.ObjectName);
            AsNoKeyArgSet = All.Values.Where(x => !x.IsKey).Stringify(Separator, x => Config.VarPrefix + x.ObjectName);
            AsFullFieldAssignmentsSet = All.Values
                .Stringify(Separator, x => $"{x.SqlName} = {Config.VarPrefix}{x.ObjectName}");
            AsNoKeyFieldAssignmentsSet = All.Values.Where(x => !x.IsKey)
                .Stringify(Separator, x => $"{x.SqlName} = {Config.VarPrefix}{x.ObjectName}");
            KeyField = All.Values.FirstOrDefault(x => x.IsKey).SqlName;
            KeyArg = Config.VarPrefix + KeyField;
            KeyInSet = $"{KeyField} IN {Config.VarPrefix}{KeyField.Pluralize()}";
            KeyEquality = $"{KeyField} = {KeyArg}";
        }

        private string Separator => Config.FieldLayoutPolicy.Separator;

        public Dictionary<string, DAQToken> All { get; private set; }

        /// <summary>
        /// Contains a full set of fields related to an entity properties 
        /// with regards to FieldLayoutPolicy and column name attributes 
        /// e.g.  <code>Id, Name, CreatedUTC</code>.
        /// </summary>
        public string AsFullFieldSet { get; }

        /// <summary>
        /// Contains a set of fields without a key field related to an entity properties 
        /// with regards to FieldLayoutPolicy and column name attributes 
        /// e.g.  <code>Name, CreatedUTC</code>
        /// </summary>
        public string AsNoKeyFieldSet { get; }

        /// <summary>
        /// Contains a full set of arguments for SQL statements with regards to FieldLayoutPolicy e.g. 
        /// <code>@Id, @Name, @CreatedUTC</code>. Column attribute does not affect this.
        /// </summary>
        public string AsFullArgSet { get; }

        /// <summary>
        /// Contains a set of arguments without a key field for UPDATE statements with regards to FieldLayoutPolicy e.g. 
        /// <code>@Name, @CreatedUTC</code>
        /// </summary>
        public string AsNoKeyArgSet { get; }

        /// <summary>
        /// Contains a full set of fields assignment SQL statements with regard to FieldLayoutPolicy and column name attributes e.g. 
        /// <code>Id = @Id, Name = @Name, CreatedUTC = @CreatedUTC</code>
        /// </summary>
        public string AsFullFieldAssignmentsSet { get; }

        /// <summary>
        /// Contains a set of fields assignment SQL statements without a key field involved 
        /// with regards to FieldLayoutPolicy and column name attributes e.g.
        /// <code>Id = @Id, Name = @Name, CreatedUTC = @CreatedUTC</code>
        /// </summary>
        public string AsNoKeyFieldAssignmentsSet { get; }

        /// <summary>
        /// Contains a key field name with regards to KeyAttribute
        /// </summary>
        public string KeyField { get; } 

        /// <summary>
        /// Contains a key argument with regards to KeyAttribute
        /// </summary>
        public string KeyArg { get; }

        /// <summary>
        /// Contains a key field IN statement for an argument of the same name but in plural form e.g.
        /// <code>Id IN @Ids</code> or <code>CustomerNumber IN @CustomerNumbers</code>
        /// </summary>
        public string KeyInSet { get; }
        public string KeyEquality { get; }

        private IEnumerable<KeyValuePair<string, DAQToken>> GetTypeTableFields(Type type)
        {
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (CheckIsProperField(prop))
                {
                    yield return KeyValuePair.Create(
                        prop.Name, 
                        new DAQToken(GetFieldName(prop), prop.Name, DefineIsKeyOrNot(prop)));
                }
            }
        }
        private bool CheckIsProperField(PropertyInfo x)
        {
            return x.CanWrite
                && x.CanRead
                && x.GetCustomAttributes().All(x => x.GetType() != Config.NotMappedType);
        }
        private string GetFieldName(PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute(Config.ColumnAttributeType);
            if (attr is null)
            {
                return prop.Name;
            }

            var attrPropNameValueOrDefault = (string?)Config.ColumnAttributeType.GetProperty("Name")?.GetValue(attr);
            var name = attrPropNameValueOrDefault ?? prop.Name;

            return name;
        }

        private bool DefineIsKeyOrNot(PropertyInfo prop)
        {
            return prop.GetCustomAttribute(Config.KeyAttributeType) is not null
                || prop.Name.Equals(Convensions.KeyFieldDefaultName, StringComparison.OrdinalIgnoreCase);
        }
    }
}