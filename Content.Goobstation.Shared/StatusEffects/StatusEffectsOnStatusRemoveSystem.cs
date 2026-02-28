using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.StatusEffects;

public sealed class StatusEffectsOnStatusRemoveSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    private readonly Dictionary<EntityUid, Dictionary<EntProtoId, TimeSpan>> _toApply = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StatusEffectsOnStatusRemoveComponent, StatusEffectRemovedEvent>(OnRemove);
    }

    private void OnRemove(Entity<StatusEffectsOnStatusRemoveComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (!_toApply.TryGetValue(args.Target, out var existing))
        {
            _toApply.Add(args.Target, ent.Comp.StatusEffects);
            return;
        }

        foreach (var (key, value) in ent.Comp.StatusEffects)
        {
            if (existing.TryGetValue(key, out var existingTime))
            {
                if (existingTime < value)
                    existing[key] = value;
            }
            else
                existing.Add(key, value);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_toApply.Count == 0)
            return;

        foreach (var (uid, dict) in _toApply)
        {
            foreach (var (key, value) in dict)
            {
                _status.TryUpdateStatusEffectDuration(uid, key, out _, value);
            }
        }

        _toApply.Clear();
    }
}
