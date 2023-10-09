using Dapper.AutoQuery.Lib.Configuration;
using Dapper.AutoQuery.Lib.Extensions;
using Humanizer;
using System.Data;
using System.Reflection;

namespace Dapper.AutoQuery.Lib.Core
{
    public class EntityMembers<T>
        where T : class
    {
        public AutoQueryConfiguration Config { get; private set; }

        public EntityMembers(AutoQueryConfiguration entityManagerConfiguration)
        {
            ArgumentNullException.ThrowIfNull(entityManagerConfiguration, nameof(Config));

            Config = entityManagerConfiguration;

            All = GetTypeTableFields(typeof(T)).ToDictionary(x => x.Key, y => y.Value);

            AsFullFieldSet = All.Values
                .Stringify(Separator, x => EscL + x + EscR);

            AsNoKeyFieldSet = All.Values
                .Where(x => !x.IsKey)
                .Stringify(Separator, x => EscL + x + EscR);

            AsFullArgSet = All.Values
                .Stringify(Separator, x => VarPrefix + x.ObjectName);

            AsNoKeyArgSet = All.Values
                .Where(x => !x.IsKey)
                .Stringify(Separator, x => VarPrefix + x.ObjectName);

            var rawKey = All.Values.FirstOrDefault(x => x.IsKey).SqlName;

            KeyField = EscL + rawKey + EscR;

            KeyArg = VarPrefix + rawKey;

            KeyInSet = $"{KeyField} IN {VarPrefix}{rawKey.Pluralize()}";

            KeyEquality = $"{KeyField} = {KeyArg}";

            AsFullFieldAssignmentsSet = All.Values
                .Stringify(Separator,
                    x => $"{EscL}{x.SqlName}{EscR} = {VarPrefix}{x.ObjectName}");

            AsNoKeyFieldAssignmentsSet = All.Values.Where(x => !x.IsKey)
                .Stringify(Separator, x => $"{EscL}{x.SqlName}{EscR} = {VarPrefix}{x.ObjectName}");
        }

        private string EscL => Config.SQLDependentConfig.EscLChar;
        private string EscR => Config.SQLDependentConfig.EscRChar;
        private string VarPrefix => Config.SQLDependentConfig.VarPrefix;
        private string Separator => Config.SQLDependentConfig.FieldLayoutPolicy.ToString();

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