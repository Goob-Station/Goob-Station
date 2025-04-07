// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Chemistry.SolutionCartridge;

public sealed class SolutionCartridgeSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _container = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SolutionCartridgeComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(Entity<SolutionCartridgeComponent> ent, ref AfterInteractEvent args)
    {
        var (uid, comp) = ent;

        var other = args.Target;

        if (!TryComp(other, out SolutionContainerManagerComponent? manager) ||
            !_container.TryGetSolution((other.Value, manager),
                comp.TargetSolution,
                out var solutionEntity,
                out var solution) || solution.Contents.Count != 0 ||
            !TryComp(other.Value, out SolutionCartridgeReceiverComponent? receiver) || !_tag.HasTag(uid, receiver.Tag))
            return;

        if (!_container.TryAddSolution(solutionEntity.Value, comp.Solution))
            return;

        args.Handled = true;
        _audio.PlayPredicted(receiver.InsertSound, other.Value, args.User);
        if (_net.IsServer)
            QueueDel(uid);
    }
}