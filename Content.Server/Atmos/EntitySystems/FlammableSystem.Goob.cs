using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Common.CCVar;
using Content.Server._Goobstation.Wizard.Systems;
using Content.Server.Atmos.Components;
using Content.Shared.Atmos.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Projectiles;
using Robust.Shared.Configuration;

namespace Content.Server.Atmos.EntitySystems;

public sealed partial class FlammableSystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    [Dependency] private readonly SpellbladeSystem _spellblade = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    private int _addHeatFirestack = 1500;
    private void InitializeGoob()
    {

        SubscribeLocalEvent<FlammableComponent, GetFireStateEvent>(OnGetFireStateEvent);
        SubscribeLocalEvent<IgniteOnCollideComponent, ProjectileHitEvent>(OnProjectileHit);

        Subs.CVar(_cfg, GoobCVars.FireStackHeat, value => _addHeatFirestack = value, true);
    }

    private void OnProjectileHit(Entity<IgniteOnCollideComponent> ent, ref ProjectileHitEvent args) // Goobstation
    {
        var otherEnt = args.Target;

        if (!TryComp(otherEnt, out FlammableComponent? flammable))
            return;

        flammable.FireStacks += ent.Comp.FireStacks;
        Ignite(otherEnt, ent, flammable);
        ent.Comp.Count--;

        if (ent.Comp.Count == 0)
            RemCompDeferred<IgniteOnCollideComponent>(ent);
    }

    private void OnGetFireStateEvent(Entity<FlammableComponent> ent, ref GetFireStateEvent args)
    {
        args.OnFire = ent.Comp.OnFire;
    }
}