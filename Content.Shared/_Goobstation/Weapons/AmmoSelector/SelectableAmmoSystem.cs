using System.Linq;
using Content.Shared.Examine;
using Content.Shared.Toggleable;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Weapons.AmmoSelector;

public sealed class UnwieldOnShootSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AmmoSelectorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AmmoSelectorComponent, AmmoSelectedMessage>(OnMessage);
        SubscribeLocalEvent<AmmoSelectorComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<AmmoSelectorComponent> ent, ref ExaminedEvent args)
    {
        if (!TryComp(ent, out BasicEntityAmmoProviderComponent? provider) ||
            !_protoManager.TryIndex(provider.Proto, out var index))
            return;

        args.PushMarkup(Loc.GetString("ammo-selector-examine-mode", ("mode", index.Name)));
    }

    private void OnMapInit(Entity<AmmoSelectorComponent> ent, ref MapInitEvent args)
    {
        if (!ent.Comp.Prototypes.Any() || !TrySetProto(ent, ent.Comp.Prototypes.First()))
            QueueDel(ent);
    }

    private void OnMessage(Entity<AmmoSelectorComponent> ent, ref AmmoSelectedMessage args)
    {
        if (!ent.Comp.Prototypes.Contains(args.ProtoId) || !TrySetProto(ent, args.ProtoId))
            return;

        _audio.PlayPvs(ent.Comp.SoundSelect, ent);
    }

    private bool TrySetProto(Entity<AmmoSelectorComponent> ent, ProtoId<SelectableAmmoPrototype> proto)
    {
        if (!TryComp(ent, out BasicEntityAmmoProviderComponent? provider))
            return false;

        if (!_protoManager.TryIndex(proto, out var index))
            return false;

        provider.Proto = index.ProtoId;

        if (index.Color != null && TryComp(ent, out AppearanceComponent? appearance))
            _appearance.SetData(ent, ToggleableLightVisuals.Color, index.Color, appearance);

        Dirty(ent);
        return true;
    }
}
