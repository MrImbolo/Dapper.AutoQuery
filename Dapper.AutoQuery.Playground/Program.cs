using Dapper.AutoQuery;
using Dapper.AutoQuery.Lib.Core;
using Dapper.AutoQuery.TestModels;

AutoQueryGenerator
    .CreateBuilder()
    .SetUpSQLServer() 
    .Build();

var a = new WhereArgs()
{
    Id = 1,
    Search = "abc"
};

var select = AutoQueryGenerator.Select<TestModel, WhereArgs>((x, y) => 
    x.Id == y.Id || 
    (  
        x.FieldWithCustomName.Equals(y.Search) && 
        x.Created >= y.From
    ));

Console.WriteLine(select);

public class WhereArgs
{
    public int Id { get; set; }
    public string Search { get; set; }
    public DateTime From { get; set; }
}
