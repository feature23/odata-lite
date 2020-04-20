using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace F23.ODataLite.Internal
{
    internal static class QueryableDatabaseProviderExtensions
    {
        public static DatabaseFacade? GetDatabase<T>(this IQueryable<T> queryable)
            where T : class
        {
            if (queryable is Microsoft.EntityFrameworkCore.Internal.InternalDbSet<T> dbSet)
            {
                var infrastructure = dbSet as IInfrastructure<IServiceProvider>;
                var serviceProvider = infrastructure.Instance;
                var dbContext = serviceProvider.GetRequiredService<ICurrentDbContext>().Context;

                return dbContext.Database;
            }

            return null;
        }

        public static bool IsSqlite(this DatabaseFacade database) =>
            database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";
    }
}
