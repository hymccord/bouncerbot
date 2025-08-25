using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace BouncerBot.Services;

public interface IHashService
{
    string HashValue(ulong value);
    bool VerifyHash(string hash, ulong value);
}

internal class HMACSHA3HashService(
    IOptions<BouncerBotOptions> options
) : IHashService
{
    private readonly HMACSHA3_512 _hmac = new(Encoding.UTF8.GetBytes(options.Value.HmacKey));

    public string HashValue(ulong value)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);

        var hash = _hmac.ComputeHash([..buffer]);
        return Convert.ToBase64String(hash);
    }

    public bool VerifyHash(string hash, ulong value)
    {
        var computedHash = HashValue(value);
        return hash.Equals(computedHash, StringComparison.Ordinal);
    }
}
