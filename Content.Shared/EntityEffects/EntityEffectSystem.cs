using Robust.Shared.Timing;

namespace Content.Shared.EntityEffects;

/// <summary>
///     (Goobstation) - Entity Effect System. Use this instead of manually calling effects.
/// </summary>

// this should've been done a long time ago ngl.
// also lowkey this is not worth the goobstation folder so i'll just leave it here.
public sealed partial class EntityEffectSystem : EntitySystem
{
    public struct EntityEffectQueueEntry
    {
        public TimeSpan Time;
        public EntityEffect Effect;
        public EntityEffectBaseArgs Args;

        public EntityEffectQueueEntry(TimeSpan time, EntityEffect effect, EntityEffectBaseArgs args)
        {
            Time = time;
            Effect = effect;
            Args = args;
        }
    }

    [Dependency] private readonly IGameTiming _timing = default!;

    private List<EntityEffectQueueEntry> _queue = new();

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        for (int i = 0; i < _queue.Count; i++)
        {
            var item = _queue[i];
            if (item.Time <= _timing.CurTime)
            {
                InvokeEffect(item.Effect, item.Args);
                _queue.RemoveAt(i);
                i--; // index dont move
            }
        }
    }

#pragma warning disable CS0618
    private void InvokeEffect(EntityEffect effect, EntityEffectBaseArgs args)
    {
        effect.Effect(args);
    }
#pragma warning restore CS0618

    public void Effect(EntityEffect effect, EntityEffectBaseArgs args)
        => Effect(effect, args, TimeSpan.FromSeconds(effect.Delay));

    public void Effect(EntityEffect effect, EntityEffectBaseArgs args, TimeSpan delay)
    {
        if (delay.TotalMilliseconds <= 0)
        {
            InvokeEffect(effect, args);
            return;
        }

        var time = _timing.CurTime + delay;
        _queue.Add(new(time, effect, args));
    }
}
