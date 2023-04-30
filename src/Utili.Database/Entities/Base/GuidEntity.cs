using System;
using System.Security.Cryptography;

namespace Utili.Database.Entities.Base;

public class GuidEntity
{
    public Guid Id { get; internal set; } = CreateCryptographicallySecureGuid();

    private static Guid CreateCryptographicallySecureGuid()
    {
        using var provider = RandomNumberGenerator.Create();
        var bytes = new byte[16];
        provider.GetBytes(bytes);
        return new Guid(bytes);
    }
}
