using Content.Shared.Humanoid;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Speech;
using Content.Shared.Speech.Muting;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.CombatMode;
using Content.Shared.Nutrition.Components;
using Content.Server.Vampiric;
using Content.Shared.Abilities.Psionics;
using Content.Server.Abilities.Psionics;
using Content.Server.Cloning.Components;
using Content.Server.Psionics.Glimmer;
using Robust.Shared.GameObjects.Components.Localization;
using Robust.Shared.Enums;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;

namespace Content.Pirate.Server._EinsteinEngines.Chat;
public sealed partial class TelepathicChatSystem
{
    public string SourceToDescriptor(EntityUid source)
    {
        var ev = new GetPsychognomicDescriptorEvent();
        RaiseLocalEvent(source, ev);

        ev.Descriptors.Add(Loc.GetString("p-descriptor-ignorant"));

        return _random.Pick(ev.Descriptors);
    }
    private void InitializePsychognomy()
    {
        SubscribeLocalEvent<HumanoidAppearanceComponent, GetPsychognomicDescriptorEvent>(DescribeHumanoid);
        SubscribeLocalEvent<GrammarComponent, GetPsychognomicDescriptorEvent>(DescribeGrammar);
        SubscribeLocalEvent<DamageableComponent, GetPsychognomicDescriptorEvent>(DescribeDamage);
        SubscribeLocalEvent<MobStateComponent, GetPsychognomicDescriptorEvent>(DescribeMobState);
        SubscribeLocalEvent<HungerComponent, GetPsychognomicDescriptorEvent>(DescribeHunger);
        SubscribeLocalEvent<PhysicsComponent, GetPsychognomicDescriptorEvent>(DescribePhysics);
        SubscribeLocalEvent<FixturesComponent, GetPsychognomicDescriptorEvent>(DescribeFixtures);
        SubscribeLocalEvent<MetempsychosisKarmaComponent, GetPsychognomicDescriptorEvent>(DescribeKarma);
        SubscribeLocalEvent<GlimmerSourceComponent, GetPsychognomicDescriptorEvent>(DescribeGlimmerSource);
        SubscribeLocalEvent<PsionicComponent, GetPsychognomicDescriptorEvent>(DescribePsion);
        SubscribeLocalEvent<InnatePsionicPowersComponent, GetPsychognomicDescriptorEvent>(DescribeInnatePsionics);
        SubscribeLocalEvent<BloodSuckerComponent, GetPsychognomicDescriptorEvent>(DescribeBloodsucker);
    }

    private void DescribeHumanoid(EntityUid uid, HumanoidAppearanceComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.Sex != Sex.Unsexed)
            ev.Descriptors.Add(component.Sex == Sex.Male ? Loc.GetString("p-descriptor-male") : Loc.GetString("p-descriptor-female"));

        // Deliberately obfuscating a bit; psionic signatures use different standards
        ev.Descriptors.Add(component.Age >= 100 ? Loc.GetString("p-descriptor-old") : Loc.GetString("p-descriptor-young"));
    }

    private void DescribeGrammar(EntityUid uid, GrammarComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.Gender == Gender.Male || component.Gender == Gender.Female)
            ev.Descriptors.Add(component.Gender == Gender.Male ? Loc.GetString("p-descriptor-masculine") : Loc.GetString("p-descriptor-feminine"));
    }

    private void DescribeDamage(EntityUid uid, DamageableComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.DamageContainerID == "CorporealSpirit")
        {
            ev.Descriptors.Add(Loc.GetString("p-descriptor-liminal"));
            if (!HasComp<HumanoidAppearanceComponent>(uid))
                ev.Descriptors.Add(Loc.GetString("p-descriptor-old"));
            return;
        }

        ev.Descriptors.Add(Loc.GetString("p-descriptor-hylic"));
    }

    private void DescribeMobState(EntityUid uid, MobStateComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.CurrentState != Shared.Mobs.MobState.Critical)
            return;

        ev.Descriptors.Add(Loc.GetString("p-descriptor-liminal"));
    }

    private void DescribeHunger(EntityUid uid, HungerComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.CurrentThreshold > HungerThreshold.Peckish)
            return;

        ev.Descriptors.Add(Loc.GetString("p-descriptor-hungry"));
    }

    private void DescribeFixtures(EntityUid uid, FixturesComponent component, GetPsychognomicDescriptorEvent ev)
    {
        foreach (var fixture in component.Fixtures.Values)
            if (fixture.CollisionMask == (int) CollisionGroup.GhostImpassable)
            {
                ev.Descriptors.Add(Loc.GetString("p-descriptor-pneumatic"));
                return;
            }
    }

    private void DescribePhysics(EntityUid uid, PhysicsComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.FixturesMass < 45)
            ev.Descriptors.Add(Loc.GetString("p-descriptor-light"));
        else if (component.FixturesMass > 70)
            ev.Descriptors.Add(Loc.GetString("p-descriptor-heavy"));
    }

    private void DescribeKarma(EntityUid uid, MetempsychosisKarmaComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.Score == 0)
            return;

        ev.Descriptors.Add(Loc.GetString("p-descriptor-cyclic"));
    }

    private void DescribeGlimmerSource(EntityUid uid, GlimmerSourceComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.GlimmerPerSecond >= 0)
            ev.Descriptors.Add(Loc.GetString("p-descriptor-emanative"));
        else
        {
            ev.Descriptors.Add(Loc.GetString("p-descriptor-vampiric"));
            ev.Descriptors.Add(Loc.GetString("p-descriptor-hungry"));
        }
    }

    // This one's also a bit of a catch-all for "lacks component"
    private void DescribePsion(EntityUid uid, PsionicComponent component, GetPsychognomicDescriptorEvent ev)
    {
        if (component.PsychognomicDescriptors.Count > 0)
        {
            foreach (var descriptor in component.PsychognomicDescriptors)
            {
                ev.Descriptors.Add(Loc.GetString(descriptor));
            }
        }

        if (!HasComp<SpeechComponent>(uid) || HasComp<MutedComponent>(uid))
            ev.Descriptors.Add(Loc.GetString("p-descriptor-dumb"));

        if (!HasComp<CombatModeComponent>(uid) || HasComp<PacifiedComponent>(uid))
            ev.Descriptors.Add(Loc.GetString("p-descriptor-passive"));

        foreach (var power in component.ActivePowers)
        {
            // TODO: Mime counts too and isn't added back to psions yet
            if (power.ID != "PyrokinesisPower" && power.ID != "NoosphericZapPower")
                continue;

            ev.Descriptors.Add(Loc.GetString("p-descriptor-kinetic"));
            return;
        }
    }

    private void DescribeInnatePsionics(EntityUid uid, InnatePsionicPowersComponent component, GetPsychognomicDescriptorEvent ev)
    {
        ev.Descriptors.Add(Loc.GetString("p-descriptor-gnostic"));
    }

    private void DescribeBloodsucker(EntityUid uid, BloodSuckerComponent component, GetPsychognomicDescriptorEvent ev)
    {
        ev.Descriptors.Add(Loc.GetString("p-descriptor-vampiric"));
    }
}
public sealed class GetPsychognomicDescriptorEvent : EntityEventArgs
{
    public List<String> Descriptors = new List<String>();
}
