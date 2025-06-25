using Content.Server.Actions;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared.Alert;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Player;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the Nox Imperii system.
/// When used, the shadowling no longer becomes affected by lightning damage.
/// </summary>
public sealed class ShadowlingNoxImperiiSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly PopupSystem _popups = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingNoxImperiiComponent, NoxImperiiEvent>(OnNoxImperii);
        SubscribeLocalEvent<ShadowlingNoxImperiiComponent, NoxImperiiDoAfterEvent>(OnNoxImperiiDoAfter);
    }

    private void OnNoxImperii(EntityUid uid, ShadowlingNoxImperiiComponent component, NoxImperiiEvent args)
    {
        var doAfter = new DoAfterArgs(
            EntityManager,
            uid,
            component.Duration,
            new NoxImperiiDoAfterEvent(),
            uid,
            used: args.Action)
        {
            CancelDuplicate = true,
            BreakOnDamage = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnNoxImperiiDoAfter(EntityUid uid, ShadowlingNoxImperiiComponent component, NoxImperiiDoAfterEvent args)
    {
        if (!TryComp<ShadowlingComponent>(args.Args.User, out var sling))
            return;

        RemComp<ShadowlingNoxImperiiComponent>(uid);
        RemComp<LightDetectionComponent>(uid);
        RemComp<LightDetectionDamageModifierComponent>(uid);

        _actions.RemoveAction(uid, args.Args.Used);
        // Reduce heat damage from other sources
        sling.HeatDamage.DamageDict["Heat"] = 10;
        sling.HeatDamageProjectileModifier.DamageDict["Heat"] = 4;

        _alerts.ClearAlert(uid, sling.AlertProto);

        // Indicates that the crew should start caring more since the Shadowling is close to ascension
        _audio.PlayGlobal(new SoundPathSpecifier("/Audio/Effects/ghost.ogg"), Filter.Broadcast(), false, AudioParams.Default.WithVolume(-2f));

        _popups.PopupEntity(Loc.GetString("shadowling-nox-imperii-done"), uid, uid, PopupType.Medium);
    }
}
