// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.DoAfter;
using Content.Shared._Shitmed.Cybernetics;
using Content.Shared._Shitmed.DoAfter;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;

namespace Content.Goobstation.Shared.DoAfter;

public sealed class DoAfterDelayMultiplierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DoAfterDelayMultiplierComponent, GetDoAfterDelayMultiplierEvent>(OnGetMultiplier);
        SubscribeLocalEvent<DoAfterDelayMultiplierComponent, BodyPartRelayedEvent<GetDoAfterDelayMultiplierEvent>>(
            OnGetBodyPartMultiplier);
    }

    private void OnGetBodyPartMultiplier(Entity<DoAfterDelayMultiplierComponent> ent,
        ref BodyPartRelayedEvent<GetDoAfterDelayMultiplierEvent> args)
    {
        if (TryComp(ent, out CyberneticsComponent? cybernetics) && cybernetics.Disabled)
            args.Args.Multiplier *= 10f;

        args.Args.Multiplier *= ent.Comp.Multiplier;
    }

    private void OnGetMultiplier(Entity<DoAfterDelayMultiplierComponent> ent, ref GetDoAfterDelayMultiplierEvent args)
    {
        args.Multiplier *= ent.Comp.Multiplier;
    }
}