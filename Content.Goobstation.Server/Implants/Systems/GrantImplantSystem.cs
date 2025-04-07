// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Implants.Components;
using Content.Shared.Implants;

namespace Content.Goobstation.Server.Implants.Systems;

/// <summary>
/// Adds implants on spawn to the entity
/// </summary>
public sealed class GrantImplantSystem : EntitySystem
{
    [Dependency] private readonly SharedSubdermalImplantSystem _implantSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GrantImplantComponent, ComponentInit>(OnInit);
    }

    public void OnInit(EntityUid uid, GrantImplantComponent comp, ComponentInit args)
    {
        _implantSystem.AddImplants(uid, comp.Implants);
    }
}