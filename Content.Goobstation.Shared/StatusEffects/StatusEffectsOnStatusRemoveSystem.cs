using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.StatusEffects;

public sealed class StatusEffectsOnStatusRemoveSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    private readonly Dictionary<EntityUid, KeyValuePair<EntProtoId, TimeSpan>> _toApply = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StatusEffectsOnStatusRemoveComponent, StatusEffectRemovedEvent>(OnRemove);
    }

    private void OnRemove(Entity<StatusEffectsOnStatusRemoveComponent> ent, ref StatusEffectRemovedEvent args)
    {
        foreach (var kvp in ent.Comp.StatusEffects)
        {
            _toApply.Add(args.Target, kvp);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_toApply.Count == 0)
            return;

        foreach (var (uid, kvp) in _toApply)
        {
            _status.TryUpdateStatusEffectDuration(uid, kvp.Key, out _, kvp.Value);
        }

        _toApply.Clear();
    }
}
