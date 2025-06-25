using Content.Server.Actions;
using Content.Server.Fluids.EntitySystems;
using Content.Shared._EE.Shadowling;
using Content.Shared.Chemistry.Components;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the Blindness Smoke ability logic.
/// The performer outputs a smoke that heals all Shadowlings and Thralls, but blinds anyone else.
/// </summary>
public sealed class ShadowlingBlindnessSmokeSystem : EntitySystem
{
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingBlindnessSmokeComponent, BlindnessSmokeEvent>(OnBlindnessSmoke);
    }

    private void OnBlindnessSmoke(EntityUid uid, ShadowlingBlindnessSmokeComponent comp, BlindnessSmokeEvent args)
    {
        var xform = Transform(uid);
        var worldPos = _transform.GetMapCoordinates(uid, xform);

        var solution = new Solution(comp.Reagent, comp.ReagentQuantity);
        var foamEnt = Spawn("Smoke", worldPos);

        _smoke.StartSmoke(foamEnt, solution, comp.Duration, comp.SpreadAmount);

        _audio.PlayPvs(comp.BlindnessSound, uid, AudioParams.Default.WithVolume(-1f));
        _actions.StartUseDelay(args.Action);
    }
}
