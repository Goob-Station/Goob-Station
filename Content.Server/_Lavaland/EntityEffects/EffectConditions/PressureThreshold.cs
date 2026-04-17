// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Lavaland.Procedural.Components;
using Content.Server.Atmos.EntitySystems;
using Robust.Shared.Prototypes;
using Content.Shared.EntityConditions;
using Content.Shared.Atmos.Components;

namespace Content.Server.EntityEffects.EffectConditions;

// I Will in fact kill someone if i have to put this under TransformComponent
public sealed partial class PressureThresholdSystem : EntityConditionSystem<MovedByPressureComponent, PressureThreshold>
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;

    protected override void Condition(Entity<MovedByPressureComponent> entity, ref EntityConditionEvent<PressureThreshold> args)
    {
        var effect = args.Condition;

        if (!TryComp<TransformComponent>(entity.Owner, out var transform))
        {
            args.Result = false;
            return;
        }

        if (effect.WorksOnLavaland && HasComp<LavalandMapComponent>(transform.MapUid))
        {
            args.Result = true;
            return;
        }

        var mix = _atmos.GetTileMixture((entity.Owner, transform));
        var pressure = mix?.Pressure ?? 0f;

        args.Result = pressure >= effect.Min && pressure <= effect.Max;
    }
}

public sealed partial class PressureThreshold : EntityConditionBase<PressureThreshold>
{
    [DataField]
    public bool WorksOnLavaland = false;

    [DataField]
    public float Min = float.MinValue;

    [DataField]
    public float Max = float.MaxValue;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-guidebook-pressure-threshold",
            ("min", Min),
            ("max", Max));
    }
}
