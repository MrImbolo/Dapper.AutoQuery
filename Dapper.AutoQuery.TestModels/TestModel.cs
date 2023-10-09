using Dapper.AutoQuery.Lib.Queries;
using Humanizer;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Dapper.AutoQuery.TestModels;

public class TestModel
{
    public TestModel()
    {

    }

    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Created { get; set; }

    [Column("CustomName")]
    public string FieldWithCustomName { get; set; }

}
 
public class Orders
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string CustomerId { get; set; }

    public int OrderStateId { get; set; }

    public string CreatedById { get; set; }

    public string LastUpdatedById { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateLastUpdated { get; set; }

}

public class OrderStates
{
    public int Id { get; set; }

    public string State { get; set; }

    public string Caption { get; set; }

}

public class ProductResources
{
    public int Id { get; set; }

    public int MaterialId { get; set; }

    public int Quantity { get; set; }

    public int ProductId { get; set; }

    public string CreatedById { get; set; }

}

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; }

    public DateTime DateCreated { get; set; }

    public string CreatedById { get; set; }

    public string CustomerId { get; set; }

    public int? OrderId { get; set; }

}

public class Resources
{
    public int Id { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }

    public int UnitId { get; set; }

    public int ResourceTypeId { get; set; }

    public string CreatedById { get; set; }

}

public class ResourceTypes
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string CreatedById { get; set; }

}

public class RoleClaims
{
    public int Id { get; set; }

    public string RoleId { get; set; }

    public string ClaimType { get; set; }

    public string ClaimValue { get; set; }

}

public class Roles
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string NormalizedName { get; set; }

    public string ConcurrencyStamp { get; set; }

}

public class Units
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Symbol { get; set; }

    public bool IsComposite { get; set; }

    public string Contains { get; set; }

    public string CultureCode { get; set; }

    public string CreatedById { get; set; }

}

public class UserClaims
{
    public int Id { get; set; }

    public string UserId { get; set; }

    public string ClaimType { get; set; }

    public string ClaimValue { get; set; }

}

public class UserDeviceCodes
{
    public string UserCode { get; set; }

    public string DeviceCode { get; set; }

    public string SubjectId { get; set; }

    public string SessionId { get; set; }

    public string ClientId { get; set; }

    public string Description { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime Expiration { get; set; }

    public string Data { get; set; }

}

public class UserLogins
{
    public string LoginProvider { get; set; }

    public string ProviderKey { get; set; }

    public string ProviderDisplayName { get; set; }

    public string UserId { get; set; }

}

public class UserPersistedGrants
{
    public string Key { get; set; }

    public string Type { get; set; }

    public string SubjectId { get; set; }

    public string SessionId { get; set; }

    public string ClientId { get; set; }

    public string Description { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? Expiration { get; set; }

    public DateTime? ConsumedTime { get; set; }

    public string Data { get; set; }

}

public class UserRoles
{
    public string UserId { get; set; }

    public string RoleId { get; set; }

}

public class Users
{
    public string Id { get; set; }

    public DateTime Created { get; set; }

    public string UserName { get; set; }

    public string NormalizedUserName { get; set; }

    public string Email { get; set; }

    public string NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string PasswordHash { get; set; }

    public string SecurityStamp { get; set; }

    public string ConcurrencyStamp { get; set; }

    public string PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

}

public class UserTokens
{
    public string UserId { get; set; }

    public string LoginProvider { get; set; }

    public string Name { get; set; }

    public string Value { get; set; }

}
public class SelectProductsWhereArgs : IWhereClauseArgs<Product>
{
    public string? Search { get; set; }
    public DateTime? DateFilter { get; set; }
    public string ToWhereClause()
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var searchableProps = IWhereClauseArgs<Product>
                .Members
                .Where(x => x.Value.PropertyType == typeof(string))
                .Select(x => $"{x.Key} LIKE '%' + @{nameof(Search)} + '%'");

            var str = string.Join(" OR ", searchableProps);

            if (str.Length > 0)
            {
                sb.Append(str);
            }
        }

        if (DateFilter is not null)
        {
            sb.Append($"AND {nameof(Product.DateCreated)} >= @{nameof(DateFilter)}");
        }

        return sb.ToString();
    }
}