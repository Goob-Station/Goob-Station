// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._EinsteinEngines.Silicon.DeadStartupButton;

/// <summary>
/// This creates a Button that can be activated after an entity, usually a silicon or an IPC, died.
/// This will activate a doAfter and then revive the entity, playing a custom afterward sound.
/// </summary>
public abstract partial class SharedDeadStartupButtonSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly INetManager _net = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<DeadStartupButtonComponent, GetVerbsEvent<AlternativeVerb>>(AddTurnOnVerb);
    }

    private void AddTurnOnVerb(EntityUid uid, DeadStartupButtonComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!_mobState.IsDead(uid)
            || !args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        if (!TryComp(uid, out MobStateComponent? mobStateComponent) || !_mobState.IsDead(uid, mobStateComponent))
            return;

        args.Verbs.Add(new AlternativeVerb()
        {
            Act = () => TryStartup(args.User, uid, component),
            Text = Loc.GetString(component.VerbText),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/Spare/poweronoff.svg.192dpi.png")),
            Priority = component.VerbPriority
        });
    }

    private void TryStartup(EntityUid user, EntityUid target, DeadStartupButtonComponent comp)
    {
        if (!_net.IsServer)
            return;
        _audio.PlayPvs(comp.ButtonSound, target);
        var args = new DoAfterArgs(EntityManager, user, comp.DoAfterInterval, new OnDoAfterButtonPressedEvent(), target, target:target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            MultiplyDelay = false, // Goobstation
        };
        _doAfterSystem.TryStartDoAfter(args);
    }

    [Serializable, NetSerializable]
    public sealed partial class OnDoAfterButtonPressedEvent : SimpleDoAfterEvent
    {
    }


}