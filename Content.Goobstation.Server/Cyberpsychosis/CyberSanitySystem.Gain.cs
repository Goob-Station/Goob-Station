using Content.Goobstation.Common.Medical;
using Content.Server.Body.Systems;
using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;

namespace Content.Goobstation.Server.Cyberpsychosis;

public sealed partial class CyberSanitySystem
{
    private void InitializeGain()
    {
        SubscribeLocalEvent<CyberSanityComponent, RecursiveBodyUpdateEvent>(OnBodyUpdate);
        SubscribeLocalEvent<CyberSanityComponent, BodyPartAddedEvent>(OnBodyUpdate);
        SubscribeLocalEvent<CyberSanityComponent, BodyPartRemovedEvent>(OnBodyUpdate);
        SubscribeLocalEvent<ModifyCyberSanityPartComponent, OrganAddedEvent>(OrganModified);
        SubscribeLocalEvent<ModifyCyberSanityPartComponent, OrganRemovedEvent>(OrganModified);

        SubscribeLocalEvent<ImmunoblockersActiveComponent, GetCyberSanityModifiersEvent>(OnGetImmunoblockersMod);
    }

    private void OnBodyUpdate<T>(Entity<CyberSanityComponent> ent, ref T args)
        => RefreshGain(ent.Owner, ent.Comp);

    private void OrganModified<T>(Entity<ModifyCyberSanityPartComponent> ent, ref T args)
    {
        if (!TryComp<OrganComponent>(ent, out var organ) || organ.Body == null)
            return;

        RefreshGain(organ.Body.Value);
    }

    private void OnGetImmunoblockersMod(Entity<ImmunoblockersActiveComponent> ent, ref GetCyberSanityModifiersEvent args)
    {
        if (args.CurrentGain >= 0)
            return;

        args.GainModifier *= ent.Comp.LossModifier;
    }

    public void RefreshGain(EntityUid uid, CyberSanityComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var gain = component.OriginalGain;

        var parts = _body.GetBodyChildren(uid);
        foreach (var part in parts)
        {
            if (TryComp<ModifyCyberSanityPartComponent>(part.Id, out var mod))
                gain += mod.ToAdd;
        }

        if (_body.TryGetBodyOrganEntityComps<ModifyCyberSanityPartComponent>(uid, out var organs))
        {
            foreach (var organ in organs)
                gain += organ.Comp1.ToAdd;
        }

        component.CurrentGain = gain;
    }

    private void GainSanity(EntityUid uid, CyberSanityComponent? comp = null, int? gain = null)
    {
        if (!Resolve(uid, ref comp, false))
            return;

        var ev = new GetCyberSanityModifiersEvent(comp.CurrentGain);
        RaiseLocalEvent(uid, ref ev);

        gain ??= (int) (comp.CurrentGain * ev.GainModifier);
        var lastSanity = comp.Sanity;
        comp.Sanity = Math.Clamp(comp.Sanity + gain.Value, 0, comp.MaxSanity);

        ApplyComponents(uid, comp, lastSanity);
        ApplyEvents(uid, comp, lastSanity);
    }

    private void ApplyComponents(EntityUid uid, CyberSanityComponent comp, int lastSanity)
    {
        foreach (var threshold in comp.ComponentThresholds)
        {
            if (lastSanity > threshold.Key == comp.Sanity > threshold.Key)
                continue;

            if (lastSanity <= threshold.Key)
                EntityManager.AddComponents(uid, threshold.Value, true);
            else if (lastSanity > threshold.Key)
                EntityManager.RemoveComponents(uid, threshold.Value);
        }
    }

    private void ApplyEvents(EntityUid uid, CyberSanityComponent comp, int lastSanity)
    {
        var events = comp.Sanity > lastSanity ? comp.EventThresholdsMore : comp.EventThresholdsLess;

        foreach (var threshold in events)
        {
            if (lastSanity > threshold.Key == comp.Sanity > threshold.Key)
                continue;

            foreach (var item in threshold.Value)
                RaiseLocalEvent(uid, item);
        }
    }
}
