using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Content.Server.Database;

internal static class EFCoreExtensions
{
    public static IQueryable<TEntity> ApplyIncludes<TEntity>(
        this IQueryable<TEntity> query,
        IEnumerable<Expression<Func<TEntity, object>>> properties)
        where TEntity : class
    {
        var q = query;
        foreach (var property in properties)
        {
            q = q.Include(property);
        }
        return q;
    }

    public static IQueryable<TEntity> ApplyIncludes<TEntity, TDerived>(
        this IQueryable<TEntity> query,
        IEnumerable<Expression<Func<TDerived, object>>> properties,
        Expression<Func<TEntity, TDerived>> getDerived)
        where TEntity : class
        where TDerived : class
    {
        var q = query;
        foreach (var property in properties)
        {
            q = q.Include(getDerived).ThenInclude(property);
        }
        return q;
    }
}
