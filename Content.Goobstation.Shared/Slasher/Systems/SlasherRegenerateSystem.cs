using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Slasher.Systems;

public sealed class SlasherRegenerateSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutions = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherRegenerateComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherRegenerateComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlasherRegenerateComponent, SlasherRegenerateEvent>(OnRegenerate);
    }

    private void OnMapInit(Entity<SlasherRegenerateComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);
        Dirty(ent);
    }

    private void OnShutdown(Entity<SlasherRegenerateComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.ActionEnt);
    }

    /// <summary>
    /// Handles the regeneration of the entity/slasher (self) & uncuffing
    /// </summary>
    /// <param name="uid">Slasher UID</param>
    /// <param name="comp">SlasherRegenerateComponent</param>
    /// <param name="args">SlasherRegenerateEvent</param>
    private void OnRegenerate(EntityUid uid, SlasherRegenerateComponent comp, SlasherRegenerateEvent args)
    {
        if (args.Handled)
            return;

        if (!comp.HasSoulAvailable)
        {
            _popup.PopupClient(Loc.GetString("slasher-regenerate-no-soul"), uid, uid);
            return;
        }

        if (_net.IsServer)
        {
            RaiseLocalEvent(uid, new RejuvenateEvent());
            TryInjectReagent(uid, comp);

            // Spawn the visual and light effect entity
            var effectEnt = Spawn(comp.RegenerateEffect, _transform.GetMapCoordinates(uid));
            _transform.SetParent(effectEnt, uid);
        }

        // Play sound effect
        _audio.PlayPredicted(comp.RegenerateSound, uid, uid);

        // Consume the soul
        comp.HasSoulAvailable = false;
        Dirty(uid, comp);

        args.Handled = true;
    }

    /// <summary>
    /// Injects the reagent into the bloodstream of the entity (self)
    /// </summary>
    /// <param name="target">The Entity calling this (self)</param>
    /// <param name="comp">The SlasherRegenerateComponent</param>
    private void TryInjectReagent(EntityUid target, SlasherRegenerateComponent comp)
    {
        if (!TryComp<BloodstreamComponent>(target, out var bloodstream))
            return;

        if (!_solutions.ResolveSolution(target, bloodstream.ChemicalSolutionName, ref bloodstream.ChemicalSolution))
            return;

        _solutions.TryAddReagent(bloodstream.ChemicalSolution.Value, new ReagentId(comp.Reagent, null), FixedPoint2.New(comp.ReagentAmount), out _);
    }

    /// <summary>
    /// Grants a soul to use for regenerate. Called when the slasher successfully steals a soul in soulsteal.
    /// </summary>
    public void GrantSoul(EntityUid uid, SlasherRegenerateComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.HasSoulAvailable = true;
        Dirty(uid, comp);
    }
}
