using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Filoroch.Template.Domain.Usuarios.Entities;

namespace Filoroch.Template.Infra.Usuarios.Mappings;

public static class UsuarioMongoMapping
{
    private static readonly GuidSerializer GuidString = new(BsonType.String);

    public static void Register()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(Usuario)))
            return;

        BsonClassMap.RegisterClassMap<Usuario>(map =>
        {
            map.AutoMap();
            map.SetIgnoreExtraElements(true);
            map.MapIdMember(x => x.Id).SetSerializer(GuidString);
        });
    }
}
