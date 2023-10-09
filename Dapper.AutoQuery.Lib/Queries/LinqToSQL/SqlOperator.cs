using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.AutoQuery.Lib.Queries.LinqToSQL
{

    public struct SqlOperation
    {
        public SqlOperation(string name, string sqlName)
        {
            Name = name;
            SqlName = sqlName;
        }
        public SqlOperation(Type? type, string name, string sqlName)
        {
            Type = type;
            Name = name;
            SqlName = sqlName;
        }
        public Type? Type { get; private set; }
        public string Name { get; private set; }
        public string SqlName { get; private set; }
    }

    public readonly struct MapSet
    {
        public MapSet(params SqlOperation[] operations)
        {
            Operations = operations;
        }

        public SqlOperation Operation => Operations.First();
        public SqlOperation[] Operations { get; }
        public readonly bool Multiple => Operations.Length > 1;

        public SqlOperation ForType(Type type)
        {
            SqlOperation? operation = Operations
                .FirstOrDefault(x => x.Type == type);

            if (operation is null)
            {
                throw new ArgumentNullException(
                    $"The instance of {nameof(MapSet)} does contain operation for type {type.GetType().Name}");
            }

            return operation.Value;
        }
    }

    public interface IOperationMap
    {
        MapSet this[string csharpOperator] { get; }
        bool TryGetMapForOperator(string cSharpOperator, out MapSet mapSet);
    }
    public class SqlServerOperationMap : IOperationMap
    {
        public MapSet this[string csharpOperator] => Ops[csharpOperator];

        internal static Dictionary<string, MapSet> Ops { get; set; } = new()
        {
            {
                "Contains",
                new MapSet(
                    new SqlOperation(typeof(string), "Contains", "LIKE"),
                    new SqlOperation(typeof(IEnumerable), "Contains", "IN"))
            },
            {
                "Equals",
                new MapSet(
                    new SqlOperation(typeof(string), "Equals", "=")
                )
            },
            { "==",         new MapSet(new SqlOperation("==", "=")) },
            { "!=",         new MapSet(new SqlOperation("!=", "<>")) },
            { "<=",         new MapSet(new SqlOperation("<=", "<=")) },
            { ">=",         new MapSet(new SqlOperation(">=", ">=")) },
        };

        public bool TryGetMapForOperator(string cSharpOperator, out MapSet mapSet) =>
            Ops.TryGetValue(cSharpOperator, out mapSet);
    }

}
