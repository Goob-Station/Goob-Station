// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server._Goobstation.Wizard.Systems;
using Content.Server.Actions;
using Content.Server.Chat.Systems;
using Content.Server.Pinpointer;
using Content.Server.Popups;
using Content.Server.Warps;
using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared._Goobstation.Wizard.Teleport;
using Content.Shared.Actions;
using Content.Shared.Magic.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard.Teleport;

public sealed class WizardTeleportSystem : SharedWizardTeleportSystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly WizardRuleSystem _wizard = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SpellsSystem _spells = default!;

    private static readonly EntProtoId SmokeProto = "AdminInstantEffectSmoke10";

    private static readonly SoundSpecifier TeleportSound =
        new SoundPathSpecifier("/Audio/_Goobstation/Wizard/teleport_diss.ogg");

    private static readonly SoundSpecifier PostTeleportSound =
        new SoundPathSpecifier("/Audio/_Goobstation/Wizard/teleport_app.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UserInterfaceComponent, WizardTeleportLocationSelectedMessage>(OnLocationSelected);

        SubscribeLocalEvent<TeleportScrollComponent, WizardTeleportLocationSelectedMessage>(OnScrollLocationSelected);
        SubscribeLocalEvent<TeleportScrollComponent, AfterActivatableUIOpenEvent>(OnAfterUIOpen);

        SubscribeLocalEvent<WizardTeleportWarpPointComponent, MapInitEvent>(OnTeleportWarpMapInit,
            after: new[] { typeof(NavMapSystem) });
    }

    private void OnLocationSelected(Entity<UserInterfaceComponent> ent, ref WizardTeleportLocationSelectedMessage args)
    {
        if (HasComp<TeleportScrollComponent>(ent))
            return;

        if (args.Action == null)
            return;

        var action = GetEntity(args.Action.Value);

        if (!TryComp(action, out InstantActionComponent? instantAction) || !_actions.ValidAction(instantAction))
            return;

        var user = args.Actor;
        var location = GetEntity(args.Location);

        if (!HasComp<WizardTeleportLocationComponent>(location))
            return;

        _spells.SpeakSpell(user,
            user,
            Loc.GetString("action-speech-spell-teleport", ("location", args.LocationName)),
            MagicSchool.Translocation);

        _actions.StartUseDelay(action);

        Teleport(user, location);
    }

    private void OnScrollLocationSelected(Entity<TeleportScrollComponent> ent,
        ref WizardTeleportLocationSelectedMessage args)
    {
        if (ent.Comp.UsesLeft <= 0)
            return;

        var user = args.Actor;
        var location = GetEntity(args.Location);

        if (!HasComp<WizardTeleportLocationComponent>(location))
            return;

        Teleport(user, location);

        ent.Comp.UsesLeft--;
        if (ent.Comp.UsesLeft <= 0)
        {
            _popup.PopupEntity(Loc.GetString("teleport-scroll-no-charges"), user, user, PopupType.Medium);
            _uiSystem.CloseUis(ent.Owner);

            // Don't Queuedel right away so that client doesn't throw debug assert exception
            var fading = EnsureComp<FadingTimedDespawnComponent>(ent.Owner);
            fading.Lifetime = 0f;
            fading.FadeOutTime = 2f;
            Dirty(ent.Owner, fading);
        }

        Dirty(ent);
    }

    private void Teleport(EntityUid user, EntityUid location)
    {
        _pullingSystem.StopAllPulls(user);

        var userXform = Transform(user);

        Spawn(SmokeProto, _transform.GetMapCoordinates(user, userXform));
        _audio.PlayPvs(TeleportSound, userXform.Coordinates);

        var coords = _transform.GetMapCoordinates(location);
        _transform.SetMapCoordinates(user, coords);

        Spawn(SmokeProto, coords);
        _audio.PlayPvs(PostTeleportSound, userXform.Coordinates);
    }

    public override void OnTeleportSpell(EntityUid performer, EntityUid action)
    {
        if (!_uiSystem.HasUi(performer, WizardTeleportUiKey.Key))
            return;

        if (!_uiSystem.IsUiOpen(performer, WizardTeleportUiKey.Key, performer))
            _uiSystem.OpenUi(performer, WizardTeleportUiKey.Key, performer);
        else
        {
            _uiSystem.CloseUi(performer, WizardTeleportUiKey.Key);
            return;
        }

        var state = new WizardTeleportState(GetWizardTeleportLocations().ToList(), GetNetEntity(action));
        _uiSystem.SetUiState(performer, WizardTeleportUiKey.Key, state);
    }

    private void OnAfterUIOpen(Entity<TeleportScrollComponent> ent, ref AfterActivatableUIOpenEvent args)
    {
        if (!_uiSystem.HasUi(ent, WizardTeleportUiKey.Key))
            return;

        var state = new WizardTeleportState(GetWizardTeleportLocations().ToList(), null);
        _uiSystem.SetUiState(ent.Owner, WizardTeleportUiKey.Key, state);
    }

    private void OnTeleportWarpMapInit(Entity<WizardTeleportWarpPointComponent> ent, ref MapInitEvent args)
    {
        var uid = ent.Owner;

        if (!TryComp(uid, out WarpPointComponent? warp))
            return;

        if (!TryComp(uid, out TransformComponent? xform))
            return;

        if (_wizard.GetWizardTargetStationGrids().Where(x => x != null).All(x => xform.ParentUid != x))
            return;

        if (!CanTeleportTo(xform))
            return;

        var teleportLocation = Spawn(null, _transform.GetMapCoordinates(uid, xform));
        EnsureComp<WizardTeleportLocationComponent>(teleportLocation).Location = warp.Location;
        _transform.AttachToGridOrMap(teleportLocation);
    }

    private IEnumerable<WizardWarp> GetWizardTeleportLocations()
    {
        var allQuery = AllEntityQuery<WizardTeleportLocationComponent, TransformComponent>();

        while (allQuery.MoveNext(out var uid, out var location, out var xform))
        {
            if (CanTeleportTo(xform))
                yield return new WizardWarp(GetNetEntity(uid), location.Location ?? Name(uid));
        }
    }

    private bool CanTeleportTo(TransformComponent xform)
    {
        foreach (var (_, fix) in _lookup.GetEntitiesInRange<FixturesComponent>(xform.Coordinates,
                     0.1f,
                     LookupFlags.Static))
        {
            if (fix.Fixtures.Any(x => x.Value.Hard && (x.Value.CollisionLayer & (int) CollisionGroup.Impassable) != 0))
                return false;
        }

        return true;
    }
}