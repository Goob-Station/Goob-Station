using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Paint;

[Serializable, NetSerializable]
public sealed class PaintLogImageRequestEvent(int round, string token) : EntityEventArgs
{
    public readonly int Round = round;
    public readonly string Token = token;
}

[Serializable, NetSerializable]
public sealed class PaintLogImageResponseEvent(string token, PaintImage? image) : EntityEventArgs
{
    public readonly string Token = token;
    public readonly PaintImage? Image = image;
}

public static class PaintLogToken
{
    public const string Prefix = "pnt";
    public const int HexLength = 12;

    public static string New()
    {
        return Prefix + Guid.NewGuid().ToString("N")[..HexLength];
    }

    public static bool TryFind(string message, out string token)
    {
        token = string.Empty;
        var start = 0;
        while ((start = message.IndexOf(Prefix, start, StringComparison.Ordinal)) >= 0)
        {
            var hexStart = start + Prefix.Length;
            if (hexStart + HexLength <= message.Length && IsHex(message, hexStart, HexLength))
            {
                token = message.Substring(start, Prefix.Length + HexLength);
                return true;
            }

            start = hexStart;
        }

        return false;
    }

    public static bool IsValid(string token)
    {
        return token.Length == Prefix.Length + HexLength
               && token.StartsWith(Prefix, StringComparison.Ordinal)
               && IsHex(token, Prefix.Length, HexLength);
    }

    private static bool IsHex(string text, int start, int length)
    {
        for (var i = start; i < start + length; i++)
        {
            if (!Uri.IsHexDigit(text[i]))
                return false;
        }

        return true;
    }
}
