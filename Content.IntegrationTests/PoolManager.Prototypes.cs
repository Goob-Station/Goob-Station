// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

#nullable enable
using System.Collections.Generic;
using System.Reflection;
using Robust.Shared.Utility;

namespace Content.IntegrationTests;

// Partial class for handling the discovering and storing test prototypes.
public static partial class PoolManager
{
    private static List<string> _testPrototypes = new();

    private const BindingFlags Flags = BindingFlags.Static
                                       | BindingFlags.NonPublic
                                       | BindingFlags.Public
                                       | BindingFlags.DeclaredOnly;

    private static void DiscoverTestPrototypes(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            foreach (var field in type.GetFields(Flags))
            {
                if (!field.HasCustomAttribute<TestPrototypesAttribute>())
                    continue;

                var val = field.GetValue(null);
                if (val is not string str)
                    throw new Exception($"TestPrototypeAttribute is only valid on non-null string fields");

                _testPrototypes.Add(str);
            }
        }
    }
}