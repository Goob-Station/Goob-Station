using Content.Goobstation.Server.Cursed.Components;
using Content.Goobstation.Shared.BlockSuicide;
using Content.Server.Speech.Components;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.Components;
using Content.Shared.Examine;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Components;
using Content.Shared.Popups;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.Cursed.Systems;

public sealed partial class CursedSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CursedComponent, ComponentInit>(OnCursedInit);
        SubscribeLocalEvent<CursedComponent, ComponentRemove>(OnComponentRemoved);
        SubscribeLocalEvent<CursedComponent, ExaminedEvent>(OnExamined);
    }

    private void OnCursedInit(EntityUid uid, CursedComponent component, ComponentInit args)
    {
        // Add all curse-related components once when the curse is applied
        EnsureComp<BlockSuicideComponent>(uid);
        EnsureComp<GodmodeComponent>(uid);
        EnsureComp<BlurryVisionComponent>(uid);
        EnsureComp<BackwardsAccentComponent>(uid);
        EnsureComp<PacifiedComponent>(uid);
        EnsureComp<BlockMovementComponent>(uid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CursedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            comp.Elapsed += frameTime;

            if (comp.Elapsed < comp.FreezeDuration)
                continue;

            // Remove movement block after freeze duration
            RemComp<BlockMovementComponent>(uid);

            // Show popup once when freeze ends
            if (!comp.HasTriggeredPopup)
            {
                _popupSystem.PopupEntity(
                    Loc.GetString("cursed-comp-flavor"),
                    uid,
                    uid,
                    PopupType.LargeCaution
                );
                comp.HasTriggeredPopup = true;
            }
        }
    }

    // Okay, this could cause issues if they had these comps BEFORE being cursed.
    // But lets me real, this is only used for an admin smite, who cares?
    private void OnComponentRemoved(EntityUid uid, CursedComponent component, ComponentRemove args)
    {
        // Clean up all curse-related components
        RemComp<BlockSuicideComponent>(uid);
        RemComp<BlockMovementComponent>(uid);
        RemComp<GodmodeComponent>(uid);
        RemComp<BlurryVisionComponent>(uid);
        RemComp<BackwardsAccentComponent>(uid);
        RemComp<PacifiedComponent>(uid);
    }

    private void OnExamined(Entity<CursedComponent> comp, ref ExaminedEvent args)
    {
        if (args.IsInDetailsRange && !_net.IsClient)
        {
            args.PushMarkup(Loc.GetString(
                "cursed-component-examined",
                ("target", Identity.Entity(comp, EntityManager))
            ));
        }
    }

    public void CurseEntity(EntityUid target, float freezeDuration)
    {
        var comp = EnsureComp<CursedComponent>(target);
        comp.FreezeDuration = freezeDuration;
        comp.Elapsed = 0f;
        comp.HasTriggeredPopup = false; // Reset popup flag for new curses
    }
}
