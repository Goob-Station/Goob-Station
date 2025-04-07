// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.StatusEffects;
using Content.Server.Xenoarchaeology.XenoArtifacts;

namespace Content.Server._Shitmed.StatusEffects;

public sealed class ActivateArtifactEffectSystem : EntitySystem
{
    [Dependency] private readonly ArtifactSystem _artifact = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<ActivateArtifactEffectComponent, ComponentInit>(OnInit);
    }
    private void OnInit(EntityUid uid, ActivateArtifactEffectComponent component, ComponentInit args)
    {
        if (!TryComp<ArtifactComponent>(uid, out var artifact))
            return;

        _artifact.TryActivateArtifact(uid, logMissing: false);
    }


}
