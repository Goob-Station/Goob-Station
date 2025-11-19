// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Hands;
using Robust.Shared.Serialization.Manager;

namespace Content.Goobstation.Shared.Held;

public sealed class HeldGrantComponentSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeldGrantComponentComponent, GotEquippedHandEvent>(OnCompEquip);
        SubscribeLocalEvent<HeldGrantComponentComponent, GotUnequippedHandEvent>(OnCompUnequip);
    }

    private void OnCompEquip(Entity<HeldGrantComponentComponent> ent, ref GotEquippedHandEvent args)
    {
        foreach (var (name, data) in ent.Comp.Components)
        {
            var newComp = (Component) _componentFactory.GetComponent(name);
            if (HasComp(args.User, newComp.GetType()))
                continue;

            object? temp = newComp;
            _serializationManager.CopyTo(data.Component, ref temp);
            EntityManager.AddComponent(args.User, (Component)temp!);

            ent.Comp.Active[name] = true; // Goobstation
        }
    }

    private void OnCompUnequip(Entity<HeldGrantComponentComponent> ent, ref GotUnequippedHandEvent args)
    {
        // Goobstation
        //if (!component.IsActive) return;

        foreach (var (name, data) in ent.Comp.Components)
        {
            // Goobstation
            if (!ent.Comp.Active.ContainsKey(name) || !ent.Comp.Active[name])
                continue;

            var newComp = (Component) _componentFactory.GetComponent(name);

            RemComp(args.User, newComp.GetType());
            ent.Comp.Active[name] = false; // Goobstation
        }

        // Goobstation
        //component.IsActive = false;
    }
}
