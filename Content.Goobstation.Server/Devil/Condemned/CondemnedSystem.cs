// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Religion;
using Content.Server.Polymorph.Systems;
using Content.Shared.Examine;
using Content.Shared.Interaction.Components;
using Content.Shared.Polymorph;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;

namespace Content.Goobstation.Server.Devil.Condemned;

public sealed partial class CondemnedSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PolymorphSystem _poly = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private readonly EntProtoId _defaultPentagramProto = "Pentagram";
    private readonly EntProtoId _defaultHandProto = "HellHand";
    private readonly SoundPathSpecifier _defaultSoundPath = new("/Audio/_Goobstation/Effects/earth_quake.ogg");
    private readonly ProtoId<PolymorphPrototype> _banishProto = "ShadowJaunt180";

    public enum CondemnedPhase : byte
    {
        Waiting,
        PentagramActive,
        HandActive,
        Complete
    }

    public enum CondemnedBehavior : byte
    {
        Delete,
        Banish,
    }

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CondemnedComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CondemnedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CondemnedComponent, ComponentRemove>(OnRemoved);
        InitializeOnDeath();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CondemnedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            switch (comp.CurrentPhase)
            {
                case CondemnedPhase.PentagramActive:
                    UpdatePentagramPhase(uid, comp, frameTime);
                    break;
                case CondemnedPhase.HandActive:
                    UpdateHandPhase(uid, comp, frameTime);
                    break;
            }
        }
    }

    private void OnStartup(EntityUid uid, CondemnedComponent comp, ComponentStartup args)
    {
        if (HasComp<WeakToHolyComponent>(uid))
            return;

        EnsureComp<WeakToHolyComponent>(uid);
        comp.WasWeakToHoly = true;

    }

    private void OnRemoved(EntityUid uid, CondemnedComponent comp, ComponentRemove  args)
    {
        if (comp.WasWeakToHoly)
            RemComp<WeakToHolyComponent>(uid);
    }

    public void StartCondemnation(
        EntityUid uid,
        CondemnedComponent? comp = null,
        bool freezeEntity = true,
        CondemnedBehavior behavior = CondemnedBehavior.Delete)
    {
        EnsureComp<CondemnedComponent>(uid);
        if (!Resolve(uid, ref comp))
            return;

        comp.CondemnOnDeath = false;


        if (freezeEntity)
        {
            comp.FreezeDuringCondemnation = true;
            EnsureComp<BlockMovementComponent>(uid);
        }

        var coords = Transform(uid).Coordinates;
        Spawn(_defaultPentagramProto, coords);
        _audio.PlayPvs(_defaultSoundPath, coords);

        if (comp.CondemnedBehavior == CondemnedBehavior.Delete)
            _popup.PopupCoordinates(Loc.GetString("condemned-start", ("target", Name(uid))), coords, PopupType.LargeCaution);

        comp.CurrentPhase = CondemnedPhase.PentagramActive;
        comp.PhaseTimer = 0f;
        comp.CondemnedBehavior = behavior;
    }

    private void UpdatePentagramPhase(EntityUid uid, CondemnedComponent comp, float frameTime)
    {
        comp.PhaseTimer += frameTime;

        if (comp.PhaseTimer < 3f)
            return;

        var coords = Transform(uid).Coordinates;
        var handEntity = Spawn(_defaultHandProto, coords);

        comp.HandDuration = TryComp<TimedDespawnComponent>(handEntity, out var timedDespawn)
            ? timedDespawn.Lifetime
            : 1f;

        comp.CurrentPhase = CondemnedPhase.HandActive;
        comp.PhaseTimer = 0f;
    }

    private void UpdateHandPhase(EntityUid uid, CondemnedComponent comp, float frameTime)
    {
        comp.PhaseTimer += frameTime;

        if (comp.PhaseTimer < comp.HandDuration)
            return;

        if (comp.FreezeDuringCondemnation)
            RemComp<BlockMovementComponent>(uid);

        TryDoCondemnedBehavior(uid, comp);

        comp.CurrentPhase = CondemnedPhase.Complete;
        RemComp<CondemnedComponent>(uid);
    }

    private void TryDoCondemnedBehavior(EntityUid uid, CondemnedComponent comp)
    {
        switch (comp)
        {
            case { CondemnedBehavior: CondemnedBehavior.Delete }:
                QueueDel(uid);
                break;
            case { CondemnedBehavior: CondemnedBehavior.Banish }:
                _poly.PolymorphEntity(uid, _banishProto);
                break;
        }
    }

    private void OnExamined(EntityUid uid, CondemnedComponent comp, ExaminedEvent args)
    {
        if (args.IsInDetailsRange && !comp.SoulOwnedNotDevil)
            args.PushMarkup(Loc.GetString("condemned-component-examined", ("target", uid)));
    }
}
