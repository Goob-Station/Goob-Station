// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Implants.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Implants;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class StypticStimulatorImplantSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StypticStimulatorImplantComponent, ImplantImplantedEvent>(OnImplant);
        SubscribeLocalEvent<StypticStimulatorImplantComponent, EntGotRemovedFromContainerMessage>(OnUnimplanted);
    }

    private void OnImplant(Entity<StypticStimulatorImplantComponent> implant, ref ImplantImplantedEvent args)
    {
        if (!args.Implanted.HasValue || TerminatingOrDeleted(args.Implanted.Value))
            return;

        implant.Comp.User = args.Implanted.Value;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StypticStimulatorImplantComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.NextExecutionTime > _gameTiming.CurTime || comp.User is not { } user)
                continue;

            if (TryComp<BloodstreamComponent>(user, out var bloodstreamComponent))
                _bloodstream.TryModifyBleedAmount(user, comp.BleedingModifier, bloodstreamComponent);

            if (TryComp<DamageableComponent>(user, out var damageableComponent))
                _damageable.TryChangeDamage(user, comp.DamageModifier, true, false, damageableComponent);

            comp.NextExecutionTime = _gameTiming.CurTime + comp.ExecutionDelay;
        }
    }

    private void OnUnimplanted(Entity<StypticStimulatorImplantComponent> implant, ref EntGotRemovedFromContainerMessage args)
    {
        implant.Comp.User = null;
    }
}
