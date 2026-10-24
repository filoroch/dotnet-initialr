using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using FluentAssertions;
using Xunit;
using Filoroch.Template.Domain.Usuarios.Entities;
using Filoroch.Template.Domain.Usuarios.Enums;
using Filoroch.Template.Infra.Usuarios.Mappings;

namespace Filoroch.Template.Domain.Tests;

public class MongoSerializationTests
{
    [Fact]
    public async Task Deve_Serializar_E_Deserializar_Usuario_Com_Guid_Como_String()
    {
        // Arrange - register the mapping
        UsuarioMongoMapping.Register();
        
        // Act - create a user
        var user = new Usuario("testuser", "test@test.com", "hash123", PerfilUsuario.Admin);
        
        // Serialize to BsonDocument using the registered mapping
        var bson = user.ToBsonDocument();
        bson.Should().NotBeNull();
        
        // Assert - verify _id is a string (Guid serialized as string)
        var bsonDoc = bson.AsBsonDocument;
        bsonDoc["_id"].Should().BeOfType<BsonString>();
        
        // Deserialize back using the same mapping
        var deserialized = await Task.FromResult(BsonSerializer.Deserialize<Usuario>(bson));
        deserialized.Should().NotBeNull();
        deserialized.Username.Should().Be("testuser");
        deserialized.Email.Should().Be("test@test.com");
        deserialized.Perfil.Should().Be(PerfilUsuario.Admin);
    }
}
