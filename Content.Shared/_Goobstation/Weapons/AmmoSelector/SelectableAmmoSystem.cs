using System.Linq;
using Content.Shared._Goobstation.Wizard.UserInterface;
using Content.Shared.Changeling;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Toggleable;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Weapons.AmmoSelector;

public sealed class SelectableAmmoSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly ActivatableUiUserWhitelistSystem _activatableUiWhitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AmmoSelectorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AmmoSelectorComponent, AmmoSelectedMessage>(OnMessage);
        SubscribeLocalEvent<AmmoSelectorComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<AmmoSelectorComponent> ent, ref ExaminedEvent args)
    {
        var name = GetProviderProtoName(ent);
        if (name == null)
            return;

        args.PushMarkup(Loc.GetString("ammo-selector-examine-mode", ("mode", name)));
    }

    private void OnMapInit(Entity<AmmoSelectorComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.Prototypes.Count > 0)
            TrySetProto(ent, ent.Comp.Prototypes.First());
    }

    private void OnMessage(Entity<AmmoSelectorComponent> ent, ref AmmoSelectedMessage args)
    {
        if (!_activatableUiWhitelist.CheckWhitelist(ent, args.Actor))
            return;

        if (!ent.Comp.Prototypes.Contains(args.ProtoId) || !TrySetProto(ent, args.ProtoId))
            return;

        var name = GetProviderProtoName(ent);
        if (name != null)
            _popup.PopupClient(Loc.GetString("mode-selected", ("mode", name)), ent, args.Actor);
        _audio.PlayPredicted(ent.Comp.SoundSelect, ent, args.Actor);
    }

    public bool TrySetProto(Entity<AmmoSelectorComponent> ent, ProtoId<SelectableAmmoPrototype> proto)
    {
        if (!_protoManager.TryIndex(proto, out var index))
            return false;

        if (!SetProviderProto(ent, index))
            return false;

        ent.Comp.CurrentlySelected = index;

        var setSound = ShouldSetSound(index);
        var setFireRate = ShouldSetFireRate(index);
        if ((setSound || setFireRate) && TryComp(ent, out GunComponent? gun))
        {
            if (setSound)
                _gun.SetSoundGunshot(gun, index.SoundGunshot);
            if (setFireRate)
                _gun.SetFireRate(gun, index.FireRate);

            _gun.RefreshModifiers((ent.Owner, gun));
        }

        if (index.Color != null && TryComp(ent, out AppearanceComponent? appearance))
            _appearance.SetData(ent, ToggleableLightVisuals.Color, index.Color, appearance);

        Dirty(ent);
        return true;
    }

    private string? GetProviderProtoName(EntityUid uid)
    {
        if (TryComp(uid, out BasicEntityAmmoProviderComponent? basic) && basic.Proto != null)
            return _protoManager.TryIndex(basic.Proto, out var index) ? index.Name : null;

        if (TryComp(uid, out ProjectileBatteryAmmoProviderComponent? projectileBattery))
            return _protoManager.TryIndex(projectileBattery.Prototype, out var index) ? index.Name : null;

        if (TryComp(uid, out ChangelingChemicalsAmmoProviderComponent? chemicals))
            return _protoManager.TryIndex(chemicals.Proto, out var index) ? index.Name : null;

        // Add more providers if needed

        return null;
    }

    private bool SetProviderProto(EntityUid uid, SelectableAmmoPrototype proto)
    {
        if (TryComp(uid, out BasicEntityAmmoProviderComponent? basic))
        {
            basic.Proto = proto.ProtoId;
            return true;
        }

        if (TryComp(uid, out ProjectileBatteryAmmoProviderComponent? projectileBattery))
        {
            projectileBattery.Prototype = proto.ProtoId;
            if (!ShouldSetFireCost(proto))
                return true;
            var oldFireCost = projectileBattery.FireCost;
            projectileBattery.FireCost = proto.FireCost;
            var fireCostDiff =  proto.FireCost / oldFireCost;
            projectileBattery.Shots = (int) Math.Round(projectileBattery.Shots / fireCostDiff);
            projectileBattery.Capacity = (int) Math.Round(projectileBattery.Capacity / fireCostDiff);
            Dirty(uid, projectileBattery);
            var updateClientAmmoEvent = new UpdateClientAmmoEvent();
            RaiseLocalEvent(uid, ref updateClientAmmoEvent);
            return true;
        }

        if (TryComp(uid, out ChangelingChemicalsAmmoProviderComponent? chemicals))
        {
            chemicals.Proto = proto.ProtoId;
            if (!ShouldSetFireCost(proto))
                return true;
            chemicals.FireCost = proto.FireCost;
            return true;
        }

        // Add more providers if needed

        return false;
    }

    private bool ShouldSetFireCost(SelectableAmmoPrototype proto)
    {
        return (proto.Flags & (int) SelectableAmmoFlags.ChangeWeaponFireCost) != 0;
    }

    private bool ShouldSetSound(SelectableAmmoPrototype proto)
    {
        return (proto.Flags & (int) SelectableAmmoFlags.ChangeWeaponFireSound) != 0;
    }

    private bool ShouldSetFireRate(SelectableAmmoPrototype proto)
    {
        return (proto.Flags & (int) SelectableAmmoFlags.ChangeWeaponFireRate) != 0;
    }
}
