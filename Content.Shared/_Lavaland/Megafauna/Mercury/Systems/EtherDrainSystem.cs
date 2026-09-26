using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

public sealed partial class EtherDrainSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;


    private readonly HashSet<Entity<ActorComponent>> _mobCache = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EtherDrainComponent, EtherDrainEvent>(OnDrain);
    }

    private void OnDrain(Entity<EtherDrainComponent> ent, ref EtherDrainEvent args) // This was supposed to be a EntityTargetActionEvent but either I'm stupid or the Megafauna system doesn't work with those.
    {
        if (args.Handled)
            return;

        var comp = ent.Comp;
        var coords = Transform(ent.Owner).Coordinates;

        _mobCache.Clear();
        _lookup.GetEntitiesInRange(coords, comp.Range, _mobCache);

        foreach (var mob in _mobCache)
        {
            if (mob.Owner == ent.Owner)
                continue;

            if (!HasComp<HumanoidAppearanceComponent>(mob))
                continue;

            _stamina.TakeOvertimeStaminaDamage(mob, comp.StaminaDrain);
            _popup.PopupPredicted(Loc.GetString("ort-ether-drain"), mob, mob, PopupType.MediumCaution);

            PredictedSpawnAtPosition(comp.Prototype, Transform(mob).Coordinates);
        }
        args.Handled = true;
    }
}
