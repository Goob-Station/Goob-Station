using System.Linq;
using Content.Shared.Changeling;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Toggleable;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Weapons.AmmoSelector;

public sealed class SelectableAmmoSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

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
        if (!ent.Comp.Prototypes.Contains(args.ProtoId) || !TrySetProto(ent, args.ProtoId))
            return;

        var name = GetProviderProtoName(ent);
        if (name != null)
            _popup.PopupEntity(Loc.GetString("mode-selected", ("mode", name)), ent, args.Actor);
        _audio.PlayPvs(ent.Comp.SoundSelect, ent);
    }

    public bool TrySetProto(Entity<AmmoSelectorComponent> ent, ProtoId<SelectableAmmoPrototype> proto)
    {
        if (!_protoManager.TryIndex(proto, out var index))
            return false;

        if (!SetProviderProto(ent, index))
            return false;

        ent.Comp.CurrentlySelected = index;

        if (index.Color != null && TryComp(ent, out AppearanceComponent? appearance))
            _appearance.SetData(ent, ToggleableLightVisuals.Color, index.Color, appearance);

        Dirty(ent);
        return true;
    }

    private string? GetProviderProtoName(EntityUid uid)
    {
        if (TryComp(uid, out BasicEntityAmmoProviderComponent? basic))
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
            projectileBattery.FireCost = proto.FireCost;
            return true;
        }

        if (TryComp(uid, out ChangelingChemicalsAmmoProviderComponent? chemicals))
        {
            chemicals.Proto = proto.ProtoId;
            chemicals.FireCost = proto.FireCost;
            return true;
        }

        // Add more providers if needed

        return false;
    }
}
