using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Goobstation.Shared.Wraith.WraithPoints;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Flash.Components;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Revenant.Components;
using Content.Shared.StatusEffect;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Goobstation.Shared.Wraith.Systems.Mobs;
public sealed partial class RalliedSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RalliedComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<RalliedComponent, GetUserMeleeDamageEvent>(OnGetMeleeDamage);
    }

    public override void Update(float frameTime)
    {
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<RalliedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // Initialize NextTick if needed
            if (comp.NextTick == default)
            {
                comp.NextTick = curTime + comp.RalliedDuration;
                Dirty(uid, comp);
            }

            foreach (var melee in EntityManager.GetComponents<MeleeWeaponComponent>(uid))
            {
                melee.NextAttack /= comp.RalliedAttackSpeed; // Increase attack speed
            }

            // Check if the effect has expired
            if (curTime >= comp.NextTick)
            {
                _popup.PopupClient("The rally effect wears off.", uid, uid, PopupType.MediumCaution);

                //Restores original attack speed
                foreach (var melee in EntityManager.GetComponents<MeleeWeaponComponent>(uid))
                {
                    melee.NextAttack *= comp.RalliedAttackSpeed;
                }

                RemComp<RalliedComponent>(uid);
            }
        }
    }

    // Modify melee damage on landing the hit, supposedly.
    private void OnGetMeleeDamage(Entity<RalliedComponent> ent, ref GetUserMeleeDamageEvent args)
    {
        var comp = ent.Comp;
        args.Damage *= comp.RalliedStrength;
    }

    private void OnMapInit(Entity<RalliedComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextTick = _timing.CurTime + ent.Comp.RalliedDuration;
        Dirty(ent);
    }

}
