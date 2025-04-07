// SPDX-FileCopyrightText: 2022 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 github-actions <github-actions@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Content.MapRenderer.Extensions
{
    public static class EnvironmentExtensions
    {
        public static bool TryGetVariable(string key, [NotNullWhen(true)] out string? value)
        {
            return (value = Environment.GetEnvironmentVariable(key)) != null;
        }

        public static string GetVariableOrThrow(string key)
        {
            return Environment.GetEnvironmentVariable(key) ??
                   throw new ArgumentException($"No environment variable found with key {key}");
        }
    }
}