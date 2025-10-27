// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text;
using JetBrains.Annotations;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.UserInterface.RichText;

/// <summary>
/// Adds a specified length of random characters that scramble at a set rate.
/// </summary>
[UsedImplicitly]
public sealed class ScrambleTag : IMarkupTag
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private const int MaxScrambleLength = 32;

    public string Name => "scramble";

    public string TextBefore(MarkupNode node)
    {
        if (!node.Attributes.TryGetValue("rate", out var rateParam) ||
            !rateParam.TryGetLong(out var rate) ||
            !node.Attributes.TryGetValue("length", out var lengthParam) ||
            !lengthParam.TryGetLong(out var length) ||
            !node.Attributes.TryGetValue("chars", out var charsParam) ||
            !charsParam.TryGetString(out var chars))
            return string.Empty;

        var seed = (int) (_timing.CurTime.TotalMilliseconds / rate);
        var rand = new Random(seed + node.GetHashCode());
        var charOptions = chars.ToCharArray();
        var realLength = MathF.Min(length.Value, MaxScrambleLength);
        var sb = new StringBuilder();
        for (var i = 0; i < realLength; i++)
        {
            var index = rand.Next() % charOptions.Length;
            sb.Append(charOptions[index]);
        }

        return sb.ToString();
    }
}