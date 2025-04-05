using System.Numerics;
using Content.Client.Animations;
using Content.Goobstation.Common.Wizard;
using Content.Goobstation.Shared.Wizard;
using Content.Goobstation.Shared.Wizard.SupermatterHalberd;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Wizard.Systems;

public sealed class SpellsSystem : SharedSpellsSystem
{
    [Dependency] private readonly ActionTargetMarkSystem _mark = default!;
    [Dependency] private readonly RaysSystem _rays = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WizardComponent, GetStatusIconsEvent>(GetWizardIcon);
        SubscribeLocalEvent<ApprenticeComponent, GetStatusIconsEvent>(GetApprenticeIcon);

        SubscribeAllEvent<ChargeSpellRaysEffectEvent>(OnChargeEffect);
    }

    private void OnChargeEffect(ChargeSpellRaysEffectEvent ev)
    {
        var uid = GetEntity(ev.Uid);

        CreateChargeEffect(uid, ev);
    }

    protected override void CreateChargeEffect(EntityUid uid, ChargeSpellRaysEffectEvent ev)
    {
        if (!Timing.IsFirstTimePredicted || uid == EntityUid.Invalid)
            return;

        var rays = _rays.DoRays(TransformSystem.GetMapCoordinates(uid),
            Color.Yellow,
            Color.Fuchsia,
            10,
            15,
            minMaxRadius: new Vector2(3f, 6f),
            proto: "EffectRayCharge",
            server: false);

        if (rays == null)
            return;

        var track = EnsureComp<TrackUserComponent>(rays.Value);
        track.User = uid;
    }

    public override void SetSwapSecondaryTarget(EntityUid user, EntityUid? target, EntityUid action)
    {
        if (target == null || user == target)
        {
            _mark.SetMark(null);
            RaisePredictiveEvent(new SetSwapSecondaryTarget(GetNetEntity(action), null));
            return;
        }

        _mark.SetMark(target);
        RaisePredictiveEvent(new SetSwapSecondaryTarget(GetNetEntity(action), GetNetEntity(target.Value)));
    }

    private void GetWizardIcon(Entity<WizardComponent> ent, ref GetStatusIconsEvent args)
    {
        var icon = (ProtoId<FactionIconPrototype>) ent.Comp.StatusIcon;
        if (ProtoMan.TryIndex(icon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }

    private void GetApprenticeIcon(Entity<ApprenticeComponent> ent, ref GetStatusIconsEvent args)
    {
        var icon = (ProtoId<FactionIconPrototype>) ent.Comp.StatusIcon;
        if (ProtoMan.TryIndex(icon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}
