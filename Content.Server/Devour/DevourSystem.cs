// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 PilgrimViis <PilgrimViis@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Lyndomen <49795619+Lyndomen@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Devour;
using Content.Shared.Devour.Components;
using Content.Shared.Humanoid;
using Content.Shared.Item; // Goobstation
using Content.Server.Nutrition.Components; // Goobstation
using Content.Shared.Whitelist;

namespace Content.Server.Devour;

public sealed class DevourSystem : SharedDevourSystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelistSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DevourerComponent, DevourDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<DevourerComponent, BeingGibbedEvent>(OnGibContents);
    }

    private void OnDoAfter(EntityUid uid, DevourerComponent component, DevourDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        var ichorInjection = new Solution(component.Chemical, component.HealRate);

        // Grant ichor if the devoured thing meets the dragon's food preference
        if (args.Args.Target != null && _entityWhitelistSystem.IsWhitelistPassOrNull(component.FoodPreferenceWhitelist, (EntityUid)args.Args.Target))
        {
            _bloodstreamSystem.TryAddToChemicals(uid, ichorInjection);
        }

        // If the devoured thing meets the stomach whitelist criteria, add it to the stomach
        if (args.Args.Target != null && _entityWhitelistSystem.IsWhitelistPass(component.StomachStorageWhitelist, (EntityUid)args.Args.Target))
        {
            ContainerSystem.Insert(args.Args.Target.Value, component.Stomach);
        }

        //TODO: Figure out a better way of removing structures via devour that still entails standing still and waiting for a DoAfter. Somehow.
        // If it's not alive, it must be a structure.
        // Delete if the thing isn't in the stomach storage whitelist (or the stomach whitelist is null/empty)
        else if (args.Args.Target != null)
        {
            QueueDel(args.Args.Target.Value);
        }

        _audioSystem.PlayPvs(component.SoundDevour, uid);
    }

    private void OnGibContents(EntityUid uid, DevourerComponent component, ref BeingGibbedEvent args)
    {
        if (component.StomachStorageWhitelist == null)
            return;

        // For some reason we have two different systems that should handle gibbing,
        // and for some another reason GibbingSystem, which should empty all containers, doesn't get involved in this process
        ContainerSystem.EmptyContainer(component.Stomach);
    }
}
