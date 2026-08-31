using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Filoroch.Template.CrossCutting.Persistence.UnitOfWork.Interfaces;
using Filoroch.Template.Domain.Usuarios.Entities;
using Filoroch.Template.Domain.Usuarios.Repositories;
using Filoroch.Template.Infra.Persistence;
using Filoroch.Template.Infra.Usuarios.Repositories;
using Filoroch.Template.IoC.Settings;

namespace Filoroch.Template.IoC.Configurations.Persistence;

public static partial class PersistenceModules
{
    static partial void AddMongo(IServiceCollection services, IConfiguration configuration, PersistenceSettings settings)
    {
        services.AddSingleton<IMongoClient>(_ => new MongoClient(settings.Providers.Mongo.ConnectionString));
        services.AddScoped(provider => provider.GetRequiredService<IMongoClient>().StartSession());
        services.AddScoped(provider => provider.GetRequiredService<IMongoClient>().GetDatabase(settings.Providers.Mongo.Database).GetCollection<Usuario>("Usuarios"));
        services.AddScoped<IUsuarioMongoRepository, UsuarioMongoRepository>();
        services.AddScoped<MongoUnitOfWork>();
        services.AddScoped<IMongoUnitOfWork>(provider => provider.GetRequiredService<MongoUnitOfWork>());
        if (settings.DefaultProvider.Equals(PersistenceProviders.Mongo, StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IUsuarioRepository>(provider => provider.GetRequiredService<IUsuarioMongoRepository>());
            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<IMongoUnitOfWork>());
        }
    }
}
