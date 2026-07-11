// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Dataset;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.BloodCult.Components;

[RegisterComponent]
public sealed partial class ShuttleCurseComponent : Component
{
    [DataField]
    public TimeSpan DelayTime = TimeSpan.FromMinutes(3);

    [DataField]
    public SoundSpecifier ScatterSound = new SoundCollectionSpecifier("GlassBreak");

    [DataField]
    public ProtoId<LocalizedDatasetPrototype> CurseMessages = "ShuttleCurse";
}
