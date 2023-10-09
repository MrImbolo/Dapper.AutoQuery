using System.Reflection;

namespace Dapper.AutoQuery.Lib.Queries
{
    public interface IWhereClauseArgs<T>
    {
        public static Dictionary<string, PropertyInfo> Members { get; } = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(x => x.Name, x => x);

        string ToWhereClause();
    }
}