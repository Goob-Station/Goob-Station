using Content.Goobstation.Shared.Loudspeaker.Components;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModLoudspeakerModuleSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void OnActivated(Entity<ModLoudspeakerModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        if (args.Wearer is not { } wearer)
            return;

        ent.Comp.User = wearer;
        var holder = EnsureComp<LoudspeakerHolderComponent>(wearer);
        if (!holder.Loudspeakers.Contains(ent))
            holder.Loudspeakers.Add(ent);
    }

    [SubscribeLocalEvent]
    private void OnDeactivated(Entity<ModLoudspeakerModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        if (ent.Comp.User is not { } wearer)
            return;

        ent.Comp.User = null;
        if (!TryComp<LoudspeakerHolderComponent>(wearer, out var holder))
            return;

        holder.Loudspeakers.Remove(ent);
        if (holder.Loudspeakers.Count == 0)
            RemComp(wearer, holder);
    }
}
