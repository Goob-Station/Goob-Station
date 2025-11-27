// SPDX-License-Identifier: MIT
//
// Shared stub for AiBuildActionEvent so that prototypes referencing
// !type:AiBuildActionEvent can be loaded by both client and server.
// The server already contains another partial definition with the
// actual build logic; this one only provides the data and attributes.

using System;
using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Factory;

/// <summary>
/// Networked WorldTargetActionEvent that requests an AI build.
/// The full logic lives in the server-side partial; this shared part
/// exists so the type is available on the client for prototype loading.
/// </summary>
public sealed partial class AiBuildActionEvent : WorldTargetActionEvent
{
    // Fields populated by prototype deserialization; names match server part.
    [DataField("duration")] public float Duration;
    [DataField("prototype")] public string Prototype = string.Empty;
    [DataField("price")] public float? Price;

    public AiBuildActionEvent() { }
}
