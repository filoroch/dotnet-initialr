using Filoroch.Template.Domain.Usuarios.Entities;
using MongoDB.Bson.Serialization;

namespace Filoroch.Template.Infra.Usuarios.Mappings;

public static class UsuarioMongoMapping
{
    public static void Register()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(Usuario)))
            BsonClassMap.RegisterClassMap<Usuario>(map => map.AutoMap());
    }
}
