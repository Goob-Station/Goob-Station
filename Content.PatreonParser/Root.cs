// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace Content.PatreonParser;

public sealed class Root
{
    [JsonPropertyName("data")]
    public Data Data = default!;

    [JsonPropertyName("included")]
    public List<Included> Included = default!;
}