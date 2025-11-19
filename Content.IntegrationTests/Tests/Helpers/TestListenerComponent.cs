// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using System.Collections.Generic;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests.Helpers;

/// <summary>
/// Component that is used by <see cref="TestListenerSystem{TEvent}"/> to store any information about received events.
/// </summary>
[RegisterComponent]
public sealed partial class TestListenerComponent : Component
{
    public Dictionary<Type, List<object>> Events = new();
}
