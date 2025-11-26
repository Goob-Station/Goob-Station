using Content.Goobstation.Common.Traits.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.IdentityManagement;
using Content.Shared.Standing;
using Content.Shared.Popups;
using Content.Shared.Stunnable;

namespace Content.Goobstation.Shared.Traits.Systems;

public sealed partial class SocialAnxietySystem : EntitySystem
{
    [Dependency] private readonly StandingStateSystem _standingSystem = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SocialAnxietyComponent, InteractionSuccessEvent>(OnHug);
    }
    private void OnHug(EntityUid uid, SocialAnxietyComponent component, ref InteractionSuccessEvent args)
    {
        _standingSystem.Down(uid);
        _stunSystem.TryStun(uid, TimeSpan.FromSeconds(component.DownedTime), true);
        var mobName = Identity.Name(uid, EntityManager);
        _popupSystem.PopupEntity(Loc.GetString("social-anxiety-hugged", ("user", mobName)), uid, PopupType.MediumCaution);
    }
}
