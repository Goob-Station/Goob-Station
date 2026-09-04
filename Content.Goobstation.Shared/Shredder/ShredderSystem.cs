using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Interaction;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Shredder;

public sealed class ShredderSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ShredderComponent, InteractUsingEvent>(OnInteract);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShredderComponent, ActiveShredderComponent>();

        while (query.MoveNext(out var uid, out var shredder, out  _))
        {
            if (shredder.FinishedShreddingTime > _timing.CurTime)
                continue;

            _appearanceSystem.SetData(uid, ShredderVisuals.VisualState, ShredderVisualsState.Normal);
            shredder.FinishedShreddingTime = TimeSpan.Zero;
            RemComp<ActiveShredderComponent>(uid);
        }
    }

    private void OnInteract(Entity<ShredderComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (!TryComp<ShreddableComponent>(args.Used, out var shreddable)
            || !_power.IsPowered(ent.Owner)
            || ent.Comp.FinishedShreddingTime != TimeSpan.Zero)
            return;

        ent.Comp.ShreddingState = shreddable.ShredderState;

        EnsureComp<ActiveShredderComponent>(ent.Owner);
        _appearanceSystem.SetData(ent.Owner, ShredderVisuals.VisualState, ShredderVisualsState.Shredding);
        _audio.PlayPredicted(ent.Comp.ShreddingSound, ent.Owner, args.User);

        ent.Comp.FinishedShreddingTime = _timing.CurTime + ent.Comp.ShreddingTime;

        if (!HasComp<BodyComponent>(args.Used))
            PredictedQueueDel(args.Used);
        else
            _body.GibBody(args.Used);
    }
}
