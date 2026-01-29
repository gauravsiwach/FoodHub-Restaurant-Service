using FoodHub.Restaurant.Application.Dtos;
using FoodHub.Restaurant.Application.Interfaces;
using FoodHub.Restaurant.Application.Queries.GetRestaurantById;
// ...existing code...
using FoodHub.Restaurant.Application.Queries.GetAllRestaurants;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FoodHub.Api.GraphQL.Queries;

[Authorize]
[ExtendObjectType("Query")]
public sealed class RestaurantQuery
{
    public async Task<IReadOnlyList<RestaurantDto>> GetAllRestaurants(
        [Service] IRestaurantRepository repository,
        [Service] Serilog.ILogger logger,
        CancellationToken cancellationToken)
    {
        logger.ForContext<RestaurantQuery>().Information("Begin: GetAllRestaurants query");

        try
        {
            var query = new GetAllRestaurantsQuery(repository);
            var restaurants = await query.ExecuteAsync(cancellationToken);

            logger.ForContext<RestaurantQuery>().Information("Success: Retrieved {Count} restaurants", restaurants.Count);
            return restaurants;
        }
        catch (Exception ex)
        {
            logger.ForContext<RestaurantQuery>().Error(ex, "Error: Failed to get all restaurants");
            throw;
        }
    }

    public async Task<RestaurantDto?> GetRestaurantById(
        Guid id,
        [Service] IRestaurantRepository repository,
        [Service] Serilog.ILogger logger,
        CancellationToken cancellationToken)
    {
        logger.ForContext<RestaurantQuery>().Information("Begin: GetRestaurantById query for {RestaurantId}", id);

        try
        {
            var query = new GetRestaurantByIdQuery(repository);
            var restaurant = await query.ExecuteAsync(id, cancellationToken);

            if (restaurant is null)
            {
                logger.ForContext<RestaurantQuery>().Information("Success: Restaurant with Id {RestaurantId} not found", id);
            }
            else
            {
                logger.ForContext<RestaurantQuery>().Information("Success: Found restaurant {RestaurantName} with Id {RestaurantId}", restaurant.Name, restaurant.Id);
            }

            return restaurant;
        }
        catch (Exception ex)
        {
            logger.ForContext<RestaurantQuery>().Error(ex, "Error: Failed to get restaurant for Id {RestaurantId}", id);
            throw;
        }
    }


}