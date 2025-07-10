namespace SharpInvoice.Core.Domain.Shared;

using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public static partial class KeyGenerator
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const int RandomLength = 6;

    public static string Generate(string prefix, string? anhor = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        var randomPart = GenerateRandomPart();
        var builder = new StringBuilder();

        builder.Append(prefix);
        builder.Append('_');
        
        if (!string.IsNullOrWhiteSpace(anhor))
        {
            var sanitizedAnchor = Sanitize(anhor);
            builder.Append(sanitizedAnchor);
            builder.Append('_');
        }

        builder.Append(randomPart);
        return builder.ToString();
    }

    private static string GenerateRandomPart()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(RandomLength);
        var builder = new StringBuilder(RandomLength);

        for (int i = 0; i < RandomLength; i++)
        {
            var index = randomBytes[i] % Alphabet.Length;
            builder.Append(Alphabet[index]);
        }
        
        return builder.ToString();
    }

    private static string Sanitize(string input)
    {
        var sanitized = input.ToLowerInvariant();
        sanitized = NonAlphanumericRegex().Replace(sanitized, "");
        sanitized = sanitized.Length > 20 ? sanitized.Substring(0, 20) : sanitized;
        return sanitized;
    }

    [GeneratedRegex("[^a-z0-9]", RegexOptions.Compiled)]
    private static partial Regex NonAlphanumericRegex();
} 