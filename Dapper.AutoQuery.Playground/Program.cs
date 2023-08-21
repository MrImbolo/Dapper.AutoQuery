using Dapper;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

var cString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TheBuilder.dev;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

IDbConnection db = new SqlConnection(cString);

var newOrders = new List<Order>()
{
    new Order(
        1,
        "21.8 New",
        DateTime.UtcNow,
        DateTime.UtcNow,
        "SYS",
        "Add something new to it"
    ),
    new Order(
        1,
        "21.8 New 1",
        DateTime.UtcNow,
        DateTime.UtcNow,
        "SYS",
        "Add something new to it too"
    ),
    new Order(
        1,
        "21.8 Old readd",
        DateTime.UtcNow,
        DateTime.UtcNow,
        "SYS",
        "Readdition of order 20.8 Aboba"
    ),
};


db.Execute("""
    INSERT INTO Orders  (
         [CompanyId]  
        ,[Name]       
        ,[CreatedUTC]
        ,[UpdatedUTC] 
        ,[UpdatedBy]  
        ,[Comments]   
    )
    VALUES
    (
        @CompanyId, 
        @Name, 
        @CreatedUTC, 
        @UpdatedUTC, 
        @UpdatedBy,
        @Comments
    )
    """, newOrders);

var orders = db.Query<Order>("SELECT * FROM Orders");

db.Execute("TRUNCATE TABLE Orders");

foreach (var order in orders)
{
    Console.WriteLine(order);
}

[Table("Orders")]
internal class Order
{
    [Key]
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = "";
    public DateTime CreatedUTC { get; set; }
    public DateTime UpdatedUTC { get; set; }
    public string UpdatedBy { get; set; } = "SYS";
    public string Comments { get; set; } = "";

    public Order()
    {
    }
    public Order(int companyId, string name, DateTime createdUTC, DateTime updatedUTC, string updatedBy, string comments)
    {
        CompanyId = companyId;
        Name = name;
        CreatedUTC = createdUTC;
        UpdatedUTC = updatedUTC;
        UpdatedBy = updatedBy;
        Comments = comments;
    }

    public override bool Equals(object? obj)
    {
        return obj is Order other &&
               CompanyId == other.CompanyId &&
               Name == other.Name &&
               CreatedUTC == other.CreatedUTC &&
               UpdatedUTC == other.UpdatedUTC &&
               UpdatedBy == other.UpdatedBy &&
               Comments == other.Comments;
    }

    public override string ToString() =>
        $"{nameof(Id)} = {Id}, " +
        $"{nameof(CompanyId)} = {CompanyId}, " +
        $"{nameof(Name)} = {Name}, " +
        $"{nameof(CreatedUTC)} = {CreatedUTC}, " +
        $"{nameof(UpdatedUTC)} = {UpdatedUTC}, " +
        $"{nameof(UpdatedBy)} = {UpdatedBy}, " +
        $"{nameof(Comments)} = {Comments}";

    public override int GetHashCode()
    {
        return HashCode.Combine(CompanyId, Name, CreatedUTC, UpdatedUTC, UpdatedBy, Comments);
    }
}
