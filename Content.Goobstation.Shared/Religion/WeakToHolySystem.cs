// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Heretic;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Religion;

public sealed partial class WeakToHolySystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;

    private readonly Dictionary<EntityUid, FixedPoint2> _originalDamageCaps = new();

    public const string ContainerID = "Biological";
    public const string TransformedContainerID = "BiologicalMetaphysical";

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
            && damageable?.DamageContainerID == ContainerID)
        {
            _damageableSystem.ChangeDamageContainer(ent, TransformedContainerID);
        }
    }

    // passive healing on runes for aviu
    private void OnCollide(Entity<HereticRitualRuneComponent> uid, ref StartCollideEvent args)
    {
        var heretic = EnsureComp<PassiveDamageComponent>(args.OtherEntity);

        if (!HasComp<WeakToHolyComponent>(args.OtherEntity) && heretic.Damage.DamageDict.TryGetValue("Holy", out _))
            return;

            // Store the original DamageCap if it hasn't already been stored
        if (!_originalDamageCaps.ContainsKey(args.OtherEntity))
        {
        _originalDamageCaps[args.OtherEntity] = heretic.DamageCap;
        }

        heretic.Damage.DamageDict.TryAdd("Holy", -10);
        heretic.DamageCap = FixedPoint2.New(0);
        DirtyEntity(args.OtherEntity);

    }

    private void OnCollideEnd(Entity<HereticRitualRuneComponent> uid, ref EndCollideEvent args)
    {
        if (!TryComp<PassiveDamageComponent>(args.OtherEntity, out var heretic))
            {
                return;
            }

        // Restore the original DamageCap if it was stored
        if (_originalDamageCaps.TryGetValue(args.OtherEntity, out var originalCap))
            {
                heretic.DamageCap = originalCap;
                _originalDamageCaps.Remove(args.OtherEntity); // Clean up after restoring
            }

        heretic.Damage.DamageDict.Remove("Holy");
        DirtyEntity(args.OtherEntity);
    }
}
