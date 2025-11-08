using Content.Shared.Atmos;
using Content.Shared.Damage.Components;
using Content.Shared.Examine;
using Content.Shared.Radiation.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Random;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

public abstract class SharedReactorPartSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPointLightSystem _lightSystem = default!;

    private readonly float _rate = 5;
    private readonly float _bias = 1.5f;

    private readonly float _threshold  = 0.5f;
    private float _accumulator = 0f;

    #region Item Methods
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReactorPartComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<ReactorPartComponent> ent, ref ExaminedEvent args)
    {
        var comp = ent.Comp;
        if (!args.IsInDetailsRange)
            return;

        using (args.PushGroup(nameof(ReactorPartComponent)))
        {
            switch(comp.NRadioactive)
            {
                case > 8:
                    args.PushMarkup(Loc.GetString("reactor-part-nrad-5"));
                    break;
                case > 6:
                    args.PushMarkup(Loc.GetString("reactor-part-nrad-4"));
                    break;
                case > 4:
                    args.PushMarkup(Loc.GetString("reactor-part-nrad-3"));
                    break;
                case > 2:
                    args.PushMarkup(Loc.GetString("reactor-part-nrad-2"));
                    break;
                case > 1:
                    args.PushMarkup(Loc.GetString("reactor-part-nrad-1"));
                    break;
                case > 0:
                    args.PushMarkup(Loc.GetString("reactor-part-nrad-0"));
                    break;
            }

            switch (comp.Radioactive)
            {
                case > 8:
                    args.PushMarkup(Loc.GetString("reactor-part-rad-5"));
                    break;
                case > 6:
                    args.PushMarkup(Loc.GetString("reactor-part-rad-4"));
                    break;
                case > 4:
                    args.PushMarkup(Loc.GetString("reactor-part-rad-3"));
                    break;
                case > 2:
                    args.PushMarkup(Loc.GetString("reactor-part-rad-2"));
                    break;
                case > 1:
                    args.PushMarkup(Loc.GetString("reactor-part-rad-1"));
                    break;
                case > 0:
                    args.PushMarkup(Loc.GetString("reactor-part-rad-0"));
                    break;
            }

            if (comp.Temperature > Atmospherics.T0C + 500)
                args.PushMarkup(Loc.GetString("reactor-part-burning"));
            else if (comp.Temperature > Atmospherics.T0C + 80)
                args.PushMarkup(Loc.GetString("reactor-part-hot"));
        }
    }

    public override void Update(float frameTime)
    {
        _accumulator += frameTime;
        if (_accumulator > _threshold)
        {
            AccUpdate();
            _accumulator = 0;
        }
    }

    private void AccUpdate()
    {
        var query = EntityQueryEnumerator<ReactorPartComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (!_entityManager.HasComponent<RadiationSourceComponent>(uid))
            {
                var radcomp = EnsureComp<RadiationSourceComponent>(uid);
                radcomp.Intensity = (component.Radioactive * 0.1f) + (component.NRadioactive * 0.15f) + (component.SpentFuel * 0.125f);
            }

            if (component.NRadioactive > 0 && !_lightSystem.TryGetLight(uid, out _))
            {
                var lightcomp = _lightSystem.EnsureLight(uid);
                _lightSystem.SetEnergy(uid, component.NRadioactive, lightcomp);
                _lightSystem.SetColor(uid, Color.FromHex("#22bbff"), lightcomp);
                _lightSystem.SetRadius(uid, 1.2f, lightcomp);
            }

            var burncomp = EnsureComp<DamageOnInteractComponent>(uid);

            if (component.Temperature == Atmospherics.T20C)
            {
                burncomp.IsDamageActive = false;
                continue;
            }

            if (Math.Abs(component.Temperature - Atmospherics.T20C)>0.1)
                component.Temperature -= (component.Temperature - Atmospherics.T20C) * 0.01f;
            else
                component.Temperature = Atmospherics.T20C;

            var damage = Math.Max((component.Temperature - Atmospherics.T0C - 80) / 20, 0);
            burncomp.IgnoreResistances = component.Temperature > Atmospherics.T0C + 500;
            burncomp.IsDamageActive = true;
            burncomp.Damage = new() { DamageDict = new() { { "Heat", damage } } };
            Dirty(uid, burncomp);
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Melts the related ReactorPart.
    /// </summary>
    /// <param name="reactorPart">Reactor part to be melted</param>
    /// <param name="reactorEnt">Reactor housing the reactor part</param>
    /// <param name="reactorSystem">The SharedNuclearReactorSystem</param>
    public void Melt(ReactorPartComponent reactorPart, Entity<NuclearReactorComponent> reactorEnt, SharedNuclearReactorSystem reactorSystem)
    {
        if (reactorPart.Melted)
            return;

        reactorPart.Melted = true;
        reactorPart.IconStateCap += "_melted_" + _random.Next(1, 4 + 1);
        reactorSystem.UpdateGridVisual(reactorEnt.Owner, reactorEnt.Comp);
        reactorPart.NeutronCrossSection = 5f;
        reactorPart.ThermalCrossSection = 20f;
        reactorPart.IsControlRod = false;

        if(reactorPart.RodType == (byte)ReactorPartComponent.RodTypes.GasChannel)
            reactorPart.GasThermalCrossSection = 0.1f;
    }

    /// <summary>
    /// Processes heat transfer within the reactor grid.
    /// </summary>
    /// <param name="reactorPart">Reactor part applying the calculations</param>
    /// <param name="reactorEnt">Reactor housing the reactor part</param>
    /// <param name="AdjacentComponents">List of reactor parts next to the reactorPart</param>
    /// <param name="reactorSystem">The SharedNuclearReactorSystem</param>
    /// <exception cref="Exception">Calculations resulted in a sub-zero value</exception>
    public void ProcessHeat(ReactorPartComponent reactorPart, Entity<NuclearReactorComponent> reactorEnt, List<ReactorPartComponent?> AdjacentComponents, SharedNuclearReactorSystem reactorSystem)
    {
        // Intercomponent calculation
        foreach (var RC in AdjacentComponents)
        {
            if (RC == null)
                continue;

            var DeltaT = reactorPart.Temperature - RC.Temperature;
            var k = (Math.Pow(10, reactorPart.PropertyThermal / 5) - 1 + (Math.Pow(10, RC.PropertyThermal / 5) - 1)) / 2;
            var A = Math.Min(reactorPart.ThermalCrossSection, RC.ThermalCrossSection);

            reactorPart.Temperature = (float)(reactorPart.Temperature - (k * A * (0.5 * 8) / reactorPart.ThermalMass * DeltaT));
            RC.Temperature = (float)(RC.Temperature - (k * A * (0.5 * 8) / RC.ThermalMass * -DeltaT));

            if (RC.Temperature < 0 || reactorPart.Temperature < 0)
                throw new Exception("ReactorPart-ReactorPart temperature calculation resulted in sub-zero value.");

            // This is where we'd put material-based temperature effects... IF WE HAD ANY
        }

        // Component-Reactor calculation
        var reactor = reactorEnt.Comp;
        if (reactor != null)
        {
            var DeltaT = reactorPart.Temperature - reactor.Temperature;

            var k = (Math.Pow(10, reactorPart.PropertyThermal / 5) - 1 + (Math.Pow(10, 7 / 5) - 1)) / 2;
            var A = reactorPart.ThermalCrossSection;

            reactorPart.Temperature = (float)(reactorPart.Temperature - (k * A * (0.5 * 8) / reactorPart.ThermalMass * DeltaT));
            reactor.Temperature = (float)(reactor.Temperature - (k * A * (0.5 * 8) / reactor.ThermalMass * -DeltaT));

            if (reactor.Temperature < 0 || reactorPart.Temperature < 0)
                throw new Exception("Reactor-ReactorPart temperature calculation resulted in sub-zero value.");

            // This is where we'd put material-based temperature effects... IF WE HAD ANY
        }
        if (reactorPart.Temperature > reactorPart.MeltingPoint && reactorPart.MeltHealth > 0)
            reactorPart.MeltHealth -= _random.Next(10, 50 + 1);
        if (reactorPart.MeltHealth <= 0)
            Melt(reactorPart, reactorEnt, reactorSystem);
    }

    /// <summary>
    /// Returns a list of neutrons from the interation of the given ReactorPart and initial neutrons.
    /// </summary>
    /// <param name="reactorPart">Reactor part applying the calculations</param>
    /// <param name="neutrons">List of neutrons to be processed</param>
    /// <param name="uid">UID of the host reactor</param>
    /// <param name="thermalEnergy">Thermal energy released from the process</param>
    /// <returns>Post-processing list of neutrons</returns>
    public virtual List<ReactorNeutron> ProcessNeutrons(ReactorPartComponent reactorPart, List<ReactorNeutron> neutrons, EntityUid uid, out float thermalEnergy)
    {
        thermalEnergy = 0;
        var flux = new List<ReactorNeutron>(neutrons);
        foreach(var neutron in flux)
        {
            if (Prob(reactorPart.PropertyDensity * _rate * reactorPart.NeutronCrossSection * _bias))
            {
                if (neutron.velocity <= 1 && Prob(_rate * reactorPart.NRadioactive * _bias)) // neutron stimulated emission
                {
                    reactorPart.NRadioactive -= 0.001f;
                    reactorPart.Radioactive += 0.0005f;
                    for (var i = 0; i < _random.Next(3, 5 + 1); i++) // was 1, 5+1
                    {
                        neutrons.Add(new() { dir = _random.NextAngle().GetDir(), velocity = _random.Next(2, 3 + 1) });
                    }
                    neutrons.Remove(neutron);
                    reactorPart.Temperature += 75f; // 50 * 0.65, SS13 value compensated for SS14's worse gas heat caps
                    thermalEnergy += 75f * reactorPart.ThermalMass;
                }
                else if (neutron.velocity <= 5 && Prob(_rate * reactorPart.Radioactive * _bias)) // stimulated emission
                {
                    reactorPart.Radioactive -= 0.001f;
                    reactorPart.SpentFuel += 0.0005f;
                    for (var i = 0; i < _random.Next(3, 5 + 1); i++)// was 1, 5+1
                    {
                        neutrons.Add(new() { dir = _random.NextAngle().GetDir(), velocity = _random.Next(1, 3 + 1) });
                    }
                    neutrons.Remove(neutron);
                    reactorPart.Temperature += 50f; // 25 * 0.65
                    thermalEnergy += 50f * reactorPart.ThermalMass;
                }
                else
                {
                    // Put control rods first so they'd have a bigger effect
                    if (reactorPart.IsControlRod)
                        neutron.velocity = 0;
                    else if (Prob(_rate * reactorPart.PropertyHard)) // reflection, based on hardness
                        // A really complicated way of saying do a 180 or a 180+/-45
                        neutron.dir = (neutron.dir.GetOpposite().ToAngle() + (_random.NextAngle() / 4) - (MathF.Tau / 8)).GetDir();
                    else
                        neutron.velocity--;

                    if (neutron.velocity <= 0)
                        neutrons.Remove(neutron);

                    reactorPart.Temperature += 1; // ... not worth the adjustment
                    thermalEnergy += 1 * reactorPart.ThermalMass;
                }
            }
        }
        if (Prob(reactorPart.NRadioactive * _rate * reactorPart.NeutronCrossSection))
        {
            var count = _random.Next(1, 5 + 1); // Was 3+1
            for (var i = 0; i < count; i++)
            {
                neutrons.Add(new() { dir = _random.NextAngle().GetDir(), velocity = 3 });
            }
            reactorPart.NRadioactive -= 0.001f;
            reactorPart.Radioactive += 0.0005f;
            //This code has been deactivated so neutrons would have a bigger impact
            //reactorPart.Temperature += 13; // 20 * 0.65
            //thermalEnergy += 13 * reactorPart.ThermalMass;
        }
        if (Prob(reactorPart.Radioactive * _rate * reactorPart.NeutronCrossSection))
        {
            var count = _random.Next(1, 5 + 1); // Was 3+1
            for (var i = 0; i < count; i++)
            {
                neutrons.Add(new() { dir = _random.NextAngle().GetDir(), velocity = _random.Next(1, 3 + 1) });
            }
            reactorPart.Radioactive -= 0.001f;
            reactorPart.SpentFuel += 0.0005f;
            //This code has been deactivated so neutrons would have a bigger impact
            //reactorPart.Temperature += 6.5f; // 10 * 0.65
            //thermalEnergy += 6.5f * reactorPart.ThermalMass;
        }

        if (reactorPart.RodType == (byte)ReactorPartComponent.RodTypes.Control)
        {
            if (!reactorPart.Melted && (reactorPart.NeutronCrossSection != reactorPart.ConfiguredInsertionLevel))
            {
                if (reactorPart.ConfiguredInsertionLevel < reactorPart.NeutronCrossSection)
                    reactorPart.NeutronCrossSection -= Math.Min(0.1f, reactorPart.NeutronCrossSection - reactorPart.ConfiguredInsertionLevel);
                else
                    reactorPart.NeutronCrossSection += Math.Min(0.1f, reactorPart.ConfiguredInsertionLevel - reactorPart.NeutronCrossSection);
                _audio.PlayPvs(new SoundPathSpecifier("/Audio/_FarHorizons/Machines/relay_click.ogg"), uid);
            }
        }

        if (reactorPart.RodType == (byte)ReactorPartComponent.RodTypes.GasChannel)
            neutrons = ProcessNeutronsGas(reactorPart, neutrons);

        neutrons ??= [];
        return neutrons;
    }

    /// <summary>
    /// Returns a list of neutrons from the interation of the gasses within the given ReactorPart and initial neutrons.
    /// </summary>
    /// <param name="reactorPart">Reactor part applying the calculations</param>
    /// <param name="neutrons">List of neutrons to be processed</param>
    /// <returns>Post-processing list of neutrons</returns>
    public virtual List<ReactorNeutron> ProcessNeutronsGas(ReactorPartComponent reactorPart, List<ReactorNeutron> neutrons) => neutrons;

    /// <summary>
    /// Returns true according to a percent chance.
    /// </summary>
    /// <param name="chance">Double, 0-100 </param>
    /// <returns></returns>
    protected bool Prob(double chance) => _random.NextDouble() <= chance / 100;

    #endregion
}