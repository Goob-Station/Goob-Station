
using Content.Goobstation.Common.Religion;
using Content.Shared.Body.Systems;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Explosion.EntitySystems;
using Content.Shared.Ghost;
using Content.Shared.Magic;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wizard.Systems;

/// <summary>
/// TODO: finish moving goob wiz spells then remove Goob after deleting SpellsSystem
/// </summary>
public sealed partial class SharedGoobSpellsSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedMagicSystem _magic = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedExplosionSystem _explosion = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    private EntityQuery<SpectralComponent> _spectralQuery = default!;

    private LocId _locFailSilicon = "spell-fail-target-silicon";
    private LocId _locFailNotDead = "spell-fail-not-dead";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ScreamForMeEvent>(OnScreamForMe);
        SubscribeLocalEvent<CorpseExplosionEvent>(OnCorpseExplosion);

        _spectralQuery = GetEntityQuery<SpectralComponent>();
    }

    private bool IsTouchSpellDenied(EntityUid target)
    {
        var ev = new BeforeCastTouchSpellEvent(target);
        RaiseLocalEvent(target, ev, true);

        return ev.Cancelled;
    }
}