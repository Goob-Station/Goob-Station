using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;

namespace Content.Goobstation.Shared.MartialArts.Effects;

public sealed partial class Performed65EntityEffectSystem : EntityEffectSystem<TransformComponent, Performed65>
{
    protected override void Effect(Entity<TransformComponent> performer, ref EntityEffectEvent<Performed65> args)
    {
        if (!TryComp(performer, out MartialArtsComboPerformedComponent? comp))
            throw new InvalidOperationException("No MartialArtsComboPerformedOnComponent on target!");

        var ev = new Performed65Event(comp.PerformedOn);
        RaiseLocalEvent(performer, ref ev);
    }
}

[UsedImplicitly]
public sealed partial class Performed65 : EntityEffectBase<Performed65>;
