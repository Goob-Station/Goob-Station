using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Terror.Systems;

public sealed class TerrorLifeShareSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TerrorLifeShareComponent, TerrorLifeShareEvent>(OnLifeShare);
    }

    private void OnLifeShare(Entity<TerrorLifeShareComponent> ent, ref TerrorLifeShareEvent args)
    {
        if (args.Target == ent.Owner)
        {
            _popup.PopupClient(Loc.GetString("terror-life-share-self-invalid"), ent.Owner, ent.Owner);
            return;
        }

        if (!HasComp<TerrorSpiderComponent>(args.Target))
        {
            _popup.PopupClient(Loc.GetString("terror-life-share-invalid-target"), ent.Owner, ent.Owner);
            return;
        }

        _damageable.TryChangeDamage(ent.Owner, ent.Comp.SelfCost, origin: ent.Owner);
        _damageable.TryChangeDamage(args.Target, ent.Comp.HealAmount, origin: ent.Owner);

        _audio.PlayPredicted(ent.Comp.Sound, ent.Owner, ent.Owner);

        args.Handled = true;
    }
}
