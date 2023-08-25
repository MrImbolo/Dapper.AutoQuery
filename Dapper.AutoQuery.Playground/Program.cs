using Dapper.AutoQuery.Lib;
using Dapper.AutoQuery.TestModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

AutoQuery.SetUpCustomColumnAttributeSupport();
AutoQuery
    .CreateBuilder()
    .SetNotMappedAttributeType(typeof(NotMappedAttribute))
    .SetKeyAttributeType(typeof(KeyAttribute))
    .SetTableAttributeType(typeof(TableAttribute))
    .SetColumnAttributeType(typeof(ColumnAttribute))
    .SetLayoutPolicy(FieldLayoutPolicy.StartCommaMultilineOneTab)
    .SetVarPrefix("@")
    .SetGlobally();

var a = new WhereArgs()
{
    Id = 1,
    Search = "abc"
};

var select = AutoQuery.Select<TestModel, WhereArgs>((x, y) => 
    x.Id == y.Id || 
    (  
        x.FieldWithCustomName.Equals(y.Search) && 
        x.Created >= y.From
    ));
 


public class WhereArgs
{
    public int Id { get; set; }
    public string Search { get; set; }
    public DateTime From { get; set; }
}
