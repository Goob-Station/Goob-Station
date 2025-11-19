// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using System.Text.Json.Serialization;

namespace Content.PatreonParser;

public sealed class Root
{
    [JsonPropertyName("data")]
    public Data Data = default!;

    [JsonPropertyName("included")]
    public List<Included> Included = default!;
}
