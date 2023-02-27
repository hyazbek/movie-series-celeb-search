﻿using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using user_details_service.Entities;
using user_details_service.Infrastructure.DBContexts;
using System.Linq.Dynamic.Core;
using user_details_service.Helpers;

namespace user_details_service.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext repositoryContext) 
        : base(repositoryContext)
    {
    }

    public async Task<User> GetUserByIdAsync(string id)
    {
        return await FindByCondition(itm => itm.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<User>> GetUsersAsync(PagingParameters userParameters,
         SortingParameters sortingParameters)
    {
        var users = FindAll();

        ApplySort(ref users, sortingParameters.SortBy);

        return await users
            .Skip((userParameters.PageNumber - 1) * userParameters.PageSize)
            .Take(userParameters.PageSize)
            .ToListAsync();
    }

    private void ApplySort(ref IQueryable<User> users, string orderByQueryString)
    {
        if (!users.Any())
            return;

        if (string.IsNullOrWhiteSpace(orderByQueryString))
        {
            users = users.OrderBy(x => x.FirstName);
            return;
        }

        var orderParams = orderByQueryString.Trim().Split(',');
        var propertyInfos = typeof(User).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var orderQueryBuilder = new StringBuilder();
        foreach (var param in orderParams)
        {
            if (string.IsNullOrWhiteSpace(param))
                continue;
            var propertyFromQueryName = param.Split(" ")[0];
            var objectProperty = propertyInfos.FirstOrDefault(pi => pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));
            if (objectProperty == null)
                continue;
            var sortingOrder = param.EndsWith(" desc") ? "descending" : "ascending";
            orderQueryBuilder.Append($"{objectProperty.Name.ToString()} {sortingOrder}, ");
        }
        var orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');
        if (string.IsNullOrWhiteSpace(orderQuery))
        {
            users = users.OrderBy(x => x.FirstName);
            return;
        }
        users = users.OrderBy(orderQuery);
    }
}