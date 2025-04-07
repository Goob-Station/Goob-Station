// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Utility;

namespace Content.Shared.DeviceNetwork;

public sealed class NetworkPayload : Dictionary<string, object?>
{
    /// <summary>
    /// Tries to get a value from the payload and checks if that value is of type  T.
    /// </summary>
    /// <typeparam name="T">The type that should be casted to</typeparam>
    /// <returns>Whether the value was present in the payload and of the required type</returns>
    public bool TryGetValue<T>(string key, [NotNullWhen(true)] out T? value)
    {
        if (this.TryCastValue(key, out T? result))
        {
            value = result;
            return true;
        }

        value = default;
        return false;
    }
}