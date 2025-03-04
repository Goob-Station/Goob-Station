using System.Runtime.InteropServices;
using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.GameTicking;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Goobstation.Religion;

public sealed partial class WeakToHolySystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeakToHolyComponent, ComponentInit>(OnCompInit);
    }

    private void OnCompInit(Entity<WeakToHolyComponent> ent, ref ComponentInit args)
    {
        if (TryComp<DamageableComponent>(ent, out var damageable) && damageable.DamageContainerID == "Biological")
            damageable.DamageContainerID = "BiologicalMetaphysical";

        _damageableSystem.SetDamageModifierSetId(ent, "ManifestedSpirit");
    }
}

// Okay you see what I'm getting at here.
// Not finished and a little supercoded but alas.

// It works for now, but we should add a method in damageable to change an entity's damage container
