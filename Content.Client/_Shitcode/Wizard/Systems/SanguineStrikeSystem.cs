// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.SanguineStrike;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Wizard.Systems;

public sealed class SanguineStrikeSystem : SharedSanguineStrikeSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SanguineStrikeComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SanguineStrikeComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(Entity<SanguineStrikeComponent> ent, ref ComponentShutdown args)
    {
        var (uid, comp) = ent;

        if (TerminatingOrDeleted(uid))
            return;

        if (!TryComp(uid, out SpriteComponent? sprite))
            return;

        sprite.Color = comp.OldColor;
    }

    private void OnStartup(Entity<SanguineStrikeComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        if (!TryComp(uid, out SpriteComponent? sprite))
            return;

        comp.OldColor = sprite.Color;
        sprite.Color = comp.Color;
    }
}