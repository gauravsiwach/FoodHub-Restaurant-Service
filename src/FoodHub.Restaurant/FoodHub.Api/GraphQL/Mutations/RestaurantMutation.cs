using FoodHub.Restaurant.Application.Commands.CreateRestaurant;
using FoodHub.Restaurant.Application.Dtos;
using FoodHub.Restaurant.Application.Interfaces;
// ...existing code...
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FoodHub.Api.GraphQL.Mutations;

[Authorize]
[ExtendObjectType("Mutation")]
public sealed class RestaurantMutation
{
    public async Task<Guid> CreateRestaurant(
        CreateRestaurantDto input,
        [Service] IRestaurantRepository repository,
        [Service] Serilog.ILogger logger,
        CancellationToken cancellationToken)
    {
        logger.ForContext<RestaurantMutation>().Information("Begin: CreateRestaurant mutation for {RestaurantName}, {City}", input.Name, input.City);

        try
        {
            var command = new CreateRestaurantCommand(repository);
            var restaurantId = await command.ExecuteAsync(input, cancellationToken);
            
            logger.ForContext<RestaurantMutation>().Information(
                "Success: Created restaurant {RestaurantName} with Id {RestaurantId}", 
                input.Name, 
                restaurantId);
            
            return restaurantId;
        }
        catch (Exception ex)
        {
            logger.ForContext<RestaurantMutation>().Error(ex, "Error: Failed to create restaurant {RestaurantName}", input.Name);
            throw;
        }
    }

 
}