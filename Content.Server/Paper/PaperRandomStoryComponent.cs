// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PoTeletubby <ajcraigaz@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Paper;
using Content.Shared.StoryGen;
using Robust.Shared.Prototypes;

namespace Content.Server.Paper;

/// <summary>
///    Adds a randomly generated story to the content of a <see cref="PaperComponent"/>
/// </summary>
[RegisterComponent, Access(typeof(PaperRandomStorySystem))]
public sealed partial class PaperRandomStoryComponent : Component
{
    /// <summary>
    /// The <see cref="StoryTemplatePrototype"/> ID to use for story generation.
    /// </summary>
    [DataField]
    public ProtoId<StoryTemplatePrototype> Template;
}