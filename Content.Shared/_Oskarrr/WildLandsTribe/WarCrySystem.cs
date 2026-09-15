// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Oskarrr.WildLandsTribe;

public sealed class WarCrySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private static readonly ProtoId<TagPrototype> AborigineTag = "WildLandsAborigine";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WarCryComponent, AborigineWarCryEvent>(OnWarCry);
    }

    private void OnWarCry(Entity<WarCryComponent> ent, ref AborigineWarCryEvent args)
    {
        if (args.Handled)
            return;

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/chime.ogg"), ent.Owner);
        _popup.PopupPredicted(Loc.GetString("aborigine-warcry"), ent.Owner, ent.Owner, PopupType.LargeCaution);

        var allies = new HashSet<EntityUid>();
        _lookup.GetEntitiesInRange(Transform(ent.Owner).Coordinates, ent.Comp.Range, allies);

        foreach (var target in allies)
        {
            if (!HasComp<MobStateComponent>(target))
                continue;

            if (!_tag.HasTag(target, AborigineTag) && target != ent.Owner)
                continue;

            foreach (var effect in ent.Comp.StatusEffectsToRemove)
                _status.TryRemoveStatusEffect(target, effect);

            if (target != ent.Owner)
                _popup.PopupEntity(Loc.GetString("aborigine-warcry-rally"), target, target, PopupType.Medium);
        }

        args.Handled = true;
    }
}
