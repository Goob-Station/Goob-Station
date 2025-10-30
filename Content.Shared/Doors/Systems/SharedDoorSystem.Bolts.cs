// SPDX-FileCopyrightText: 2024 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ScarKy0 <scarky0@onet.eu>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Prying.Components;
using Content.Shared.Tools.Components;
using Content.Shared.Wires;

namespace Content.Shared.Doors.Systems;

public abstract partial class SharedDoorSystem
{
    public void InitializeBolts()
    {
        base.Initialize();

        SubscribeLocalEvent<DoorBoltComponent, BeforeDoorOpenedEvent>(OnBeforeDoorOpened);
        SubscribeLocalEvent<DoorBoltComponent, BeforeDoorClosedEvent>(OnBeforeDoorClosed);
        SubscribeLocalEvent<DoorBoltComponent, BeforeDoorDeniedEvent>(OnBeforeDoorDenied);
        SubscribeLocalEvent<DoorBoltComponent, BeforePryEvent>(OnDoorPry);
        SubscribeLocalEvent<DoorBoltComponent, DoorStateChangedEvent>(OnStateChanged);

        SubscribeLocalEvent<DoorBoltComponent, InteractUsingEvent>(OnInteractUsingEvent); // Goobstation - Unbolting unpowered door with wrench
        SubscribeLocalEvent<DoorBoltComponent, ManualBoltingDoAfterEvent>(OnManualBolting);
    }

    private void OnDoorPry(EntityUid uid, DoorBoltComponent component, ref BeforePryEvent args)
    {
        if (args.Cancelled)
            return;

        if (!component.BoltsDown || args.Force)
            return;

        args.Message = "airlock-component-cannot-pry-is-bolted-message";

        args.Cancelled = true;
    }

    private void OnBeforeDoorOpened(EntityUid uid, DoorBoltComponent component, BeforeDoorOpenedEvent args)
    {
        if (component.BoltsDown)
            args.Cancel();
    }

    private void OnBeforeDoorClosed(EntityUid uid, DoorBoltComponent component, BeforeDoorClosedEvent args)
    {
        if (component.BoltsDown)
            args.Cancel();
    }

    private void OnBeforeDoorDenied(EntityUid uid, DoorBoltComponent component, BeforeDoorDeniedEvent args)
    {
        if (component.BoltsDown)
            args.Cancel();
    }

    public void SetBoltWireCut(Entity<DoorBoltComponent> ent, bool value)
    {
        ent.Comp.BoltWireCut = value;
        Dirty(ent, ent.Comp);
    }

    public void UpdateBoltLightStatus(Entity<DoorBoltComponent> ent)
    {
        AppearanceSystem.SetData(ent, DoorVisuals.BoltLights, GetBoltLightsVisible(ent));
    }

    public bool GetBoltLightsVisible(Entity<DoorBoltComponent> ent)
    {
        return ent.Comp.BoltLightsEnabled &&
               ent.Comp.BoltsDown &&
               ent.Comp.Powered;
    }

    public void SetBoltLightsEnabled(Entity<DoorBoltComponent> ent, bool value)
    {
        if (ent.Comp.BoltLightsEnabled == value)
            return;

        ent.Comp.BoltLightsEnabled = value;
        Dirty(ent, ent.Comp);
        UpdateBoltLightStatus(ent);
    }

    public void SetBoltsDown(Entity<DoorBoltComponent> ent, bool value, EntityUid? user = null, bool predicted = false)
    {
        TrySetBoltDown(ent, value, user, predicted);
    }

    public bool TrySetBoltDown(
        Entity<DoorBoltComponent> ent,
        bool value,
        EntityUid? user = null,
        bool predicted = false,
        bool requirePower = true // Goobstation - Manual bolt interact with unpowered door
    )
    {
        // Goobstation - Power check
        if (!_powerReceiver.IsPowered(ent.Owner) && requirePower)
            return false;
        if (ent.Comp.BoltsDown == value)
            return false;

        ent.Comp.BoltsDown = value;
        Dirty(ent, ent.Comp);
        UpdateBoltLightStatus(ent);

        // used to reset the auto-close timer after unbolting
        var ev = new DoorBoltsChangedEvent(value);
        RaiseLocalEvent(ent.Owner, ev);

        var sound = value ? ent.Comp.BoltDownSound : ent.Comp.BoltUpSound;
        if (predicted)
            Audio.PlayPredicted(sound, ent, user: user);
        else
            Audio.PlayPvs(sound, ent);
        return true;
    }

    private void OnStateChanged(Entity<DoorBoltComponent> entity, ref DoorStateChangedEvent args)
    {
        // If the door is closed, we should look if the bolt was locked while closing
        UpdateBoltLightStatus(entity);
    }

    public bool IsBolted(EntityUid uid, DoorBoltComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
        {
            return false;
        }

        return component.BoltsDown;
    }

    // Goobstation - Start - Unbolting and bolting unpowered door with wrench
    public void OnInteractUsingEvent(Entity<DoorBoltComponent> entity, ref InteractUsingEvent args)
    {
        // Can't bolt interact with powered door
        if (args.Handled
            || entity.Comp.Powered
            && !entity.Comp.BoltWireCut
            || !TryComp(args.Used, out ToolComponent? toolComp)
            || !_toolsSystem.HasQuality(args.Used, entity.Comp.UnboltToolQuality)
            || !_sharedWiresSystem.IsPanelOpen(entity.Owner))
            return;

        var efficientToolTime = entity.Comp.ManualUnboltTime / toolComp.SpeedModifier;
        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, efficientToolTime, new ManualBoltingDoAfterEvent(), entity, entity);

        if (_doAfterSystem.TryStartDoAfter(doAfterArgs))
        {
            Audio.PlayPredicted(toolComp.UseSound, entity, args.User);
            args.Handled = true;
        }
    }

    public void OnManualBolting(Entity<DoorBoltComponent> entity, ref ManualBoltingDoAfterEvent args)
    {
        if (args.Cancelled || entity.Comp.Powered && !entity.Comp.BoltWireCut)
            return;

        TrySetBoltDown(entity, !entity.Comp.BoltsDown, args.User, true, false);
    }
    // Goobstation - End
}
