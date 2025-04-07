// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using Content.Shared.FixedPoint;

namespace Content.Server.Administration.Logs.Converters;

[AdminLogConverter]
public sealed class FixedPoint2Converter : AdminLogConverter<FixedPoint2>
{
    public override void Write(Utf8JsonWriter writer, FixedPoint2 value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Int());
    }
}