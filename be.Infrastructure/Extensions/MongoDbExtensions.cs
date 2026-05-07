using be.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace be.Infrastructure.Extensions;

public static class MongoDbExtensions
{
    private static bool _registeredConvention;

    public static IServiceCollection AddInfrastructureMongo(this IServiceCollection services)
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        services.AddSingleton<ReadContext>();

        if (!_registeredConvention)
        {
            var conventionPack = new ConventionPack
            {
                new CamelCaseElementNameConvention()
            };

            ConventionRegistry.Register(
                "camelCase",
                conventionPack,
                t => t.Namespace?.StartsWith("be.Domain.Documents") ?? false
            );

            _registeredConvention = true;
        }

        return services;
    }
}