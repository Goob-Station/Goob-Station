// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Devil.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Objectives.Components;

namespace Content.Goobstation.Server.Devil.Objectives.Systems;

public sealed partial class DevilObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SignContractConditionComponent, ObjectiveGetProgressEvent>(OnContractGetProgress);
        SubscribeLocalEvent<MeetContractWeightConditionComponent, ObjectiveGetProgressEvent>(OnWeightGetProgress);
    }

    private void OnContractGetProgress(EntityUid uid, SignContractConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = _number.GetTarget(uid);
        args.Progress = target != 0 ? MathF.Min((float)comp.ContractsSigned / target, 1f) : 1f;
    }

    private void OnWeightGetProgress(EntityUid uid, MeetContractWeightConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = _number.GetTarget(uid);
        args.Progress = target != 0 ? MathF.Min((float)comp.ContractWeight / target, 1f) : 1f;
    }
}
