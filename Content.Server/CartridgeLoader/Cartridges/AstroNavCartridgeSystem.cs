// SPDX-FileCopyrightText: 2024 ArchRBX <5040911+ArchRBX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 archrbx <punk.gear5260@fastmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.CartridgeLoader;
using Content.Shared.GPS.Components;

namespace Content.Server.CartridgeLoader.Cartridges;

public sealed class AstroNavCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoaderSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AstroNavCartridgeComponent, CartridgeAddedEvent>(OnCartridgeAdded);
        SubscribeLocalEvent<AstroNavCartridgeComponent, CartridgeRemovedEvent>(OnCartridgeRemoved);
    }

    private void OnCartridgeAdded(Entity<AstroNavCartridgeComponent> ent, ref CartridgeAddedEvent args)
    {
        EnsureComp<HandheldGPSComponent>(args.Loader);
    }

    private void OnCartridgeRemoved(Entity<AstroNavCartridgeComponent> ent, ref CartridgeRemovedEvent args)
    {
        // only remove when the program itself is removed
        if (!_cartridgeLoaderSystem.HasProgram<AstroNavCartridgeComponent>(args.Loader))
        {
            RemComp<HandheldGPSComponent>(args.Loader);
        }
    }
}