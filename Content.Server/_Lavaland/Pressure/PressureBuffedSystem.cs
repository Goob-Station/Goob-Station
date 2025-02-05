using Content.Server.Atmos.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._Lavaland.Pressure;

public sealed partial class PressureBuffedSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PressureBuffedComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<PressureBuffedComponent, GetMeleeDamageEvent>(OnGetDamage);
    }

    public void OnExamined(Entity<PressureBuffedComponent> ent, ref ExaminedEvent args)
    {
        var min = ent.Comp.RequiredPressure.X;
        var max = Math.Round(ent.Comp.RequiredPressure.Y, MidpointRounding.ToZero);
        var modifier = ent.Comp.AppliedModifier;

        var markup = Loc.GetString("lavaland-examine-pressure-buff", ("min", min), ("max", max), ("buff", modifier));
        args.PushMarkup(markup);
    }

    private void OnGetDamage(Entity<PressureBuffedComponent> ent, ref GetMeleeDamageEvent args)
    {
        var mix = _atmos.GetTileMixture((ent.Owner, Transform(ent)));
        var minmax = ent.Comp.RequiredPressure;

        var pressure = mix?.Pressure ?? 0f; // can't get any lower than 0, right?...
        var isInThresholds = pressure >= minmax.X && pressure <= minmax.Y;

        if (!isInThresholds)
            return;

        args.Damage *= ent.Comp.AppliedModifier;
    }
}
