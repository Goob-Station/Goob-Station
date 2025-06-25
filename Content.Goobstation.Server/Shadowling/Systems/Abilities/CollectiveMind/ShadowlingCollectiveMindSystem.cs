using Content.Server.Actions;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared._EE.Shadowling;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Server.GameObjects;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the logic of the Collective Mind ability.
/// The Collective Mind ability lets you gain new actions, and informs you
/// how many Thralls are required to ascend. At the same time, it stuns all Thralls for a very short amount of time.
/// </summary>
public sealed class ShadowlingCollectiveMindSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly PopupSystem _popups = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingCollectiveMindComponent, CollectiveMindEvent>(OnCollectiveMind);
    }

    private void OnCollectiveMind(EntityUid uid, ShadowlingCollectiveMindComponent comp, CollectiveMindEvent args)
    {
        if (!TryComp<ActionsComponent>(uid, out var actions))
            return;

        if (!TryComp<ShadowlingComponent>(uid, out var sling))
            return;

        if (comp.AbilitiesAdded >= comp.Locked.Count)
        {
            _popups.PopupEntity(Loc.GetString("shadowling-collective-mind-ascend"), uid, uid, PopupType.Medium);
            return;
        }

        comp.AmountOfThralls = sling.Thralls.Count;
        var thrallsRemaining = comp.ThrallsRequiredForAscension - comp.AmountOfThralls; // aka Thralls required for ascension

        if (thrallsRemaining < 0)
            thrallsRemaining = 0;

        var abiltiesAddedCount = 0;

        // Can we gain this power?
        foreach (var actionData in comp.Locked)
        {
            if (comp.AmountOfThralls >= actionData.UnlockAtThralls)
            {
                if (actionData.Added)
                    continue;

                _actions.AddAction(args.Performer, actionData.ActionPrototype, actionData.ActionEntity, component: actions);

                var componentToAdd = _compFactory.GetComponent(actionData.ActionComponentName);
                EntityManager.AddComponent(args.Performer, componentToAdd);

                ++abiltiesAddedCount;
                ++comp.AbilitiesAdded;
                actionData.Added = true;
            }
        }

         if (abiltiesAddedCount > 0)
         {
             _popups.PopupEntity(
                 Loc.GetString("shadowling-collective-mind-success", ("thralls", thrallsRemaining)),
                 uid,
                 uid,
                 PopupType.Medium);
             var effectEnt = Spawn(comp.CollectiveMindEffect, _transform.GetMapCoordinates(uid));
             _transform.SetParent(effectEnt, uid);
         }
        else
        {
            _popups.PopupEntity(Loc.GetString("shadowling-collective-mind-failure", ("thralls", thrallsRemaining)),
                uid,
                uid,
                PopupType.Medium);
            return;
        }

        // Stun starts here.
        // Scales with amount of abilities added.
        // If no abilities were added, nothing happens as seen from the return statement above.
        foreach (var thrall in sling.Thralls)
        {
            if (!HasComp<StatusEffectsComponent>(thrall))
                return;

            _stun.TryParalyze(thrall, TimeSpan.FromSeconds(comp.BaseStunTime * abiltiesAddedCount + 1), false);
        }
    }
}
