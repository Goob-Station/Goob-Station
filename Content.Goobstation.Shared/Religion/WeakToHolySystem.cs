// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Heretic;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Religion;

public sealed partial class WeakToHolySystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    private readonly Dictionary<EntityUid, FixedPoint2> _originalDamageCaps = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeakToHolyComponent, ComponentInit>(OnCompInit);
        SubscribeLocalEvent<HereticRitualRuneComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<HereticRitualRuneComponent, EndCollideEvent>(OnCollideEnd);
    }

    private void OnCompInit(Entity<WeakToHolyComponent> ent, ref ComponentInit args)
    {
        if (TryComp<DamageableComponent>(ent, out var damageable)
            && damageable?.DamageContainerID == "Biological")
        {
            _damageableSystem.ChangeDamageContainer(ent, "BiologicalMetaphysical");
        }
    }

    // passive healing on runes for aviu
    private void OnCollide(EntityUid uid, HereticRitualRuneComponent component, ref StartCollideEvent args)
    {
        var _heretic = EnsureComp<PassiveDamageComponent>(args.OtherEntity);

        if (!HasComp<WeakToHolyComponent>(args.OtherEntity) && _heretic.Damage.DamageDict.TryGetValue("Holy", out var holy)) {
            return;
        }

            // Store the original DamageCap if it hasn't already been stored
        if (!_originalDamageCaps.ContainsKey(args.OtherEntity))
        {
        _originalDamageCaps[args.OtherEntity] = _heretic.DamageCap;
        }

        _heretic.Damage.DamageDict.TryAdd("Holy", -10);
        _heretic.DamageCap = FixedPoint2.New(0);
        DirtyEntity(args.OtherEntity);

    }

    private void OnCollideEnd(EntityUid uid, HereticRitualRuneComponent component, ref EndCollideEvent args)
    {
        if (TryComp<PassiveDamageComponent>(args.OtherEntity, out var _heretic))
        {
            // Restore the original DamageCap if it was stored
            if (_originalDamageCaps.TryGetValue(args.OtherEntity, out var originalCap))
            {
                _heretic.DamageCap = originalCap;
                _originalDamageCaps.Remove(args.OtherEntity); // Clean up after restoring
            }

        _heretic.Damage.DamageDict.Remove("Holy");
        DirtyEntity(args.OtherEntity);
        }
    }
}
