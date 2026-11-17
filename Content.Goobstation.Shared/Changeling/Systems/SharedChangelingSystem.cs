// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Goobstation.Shared.Overlays;
using Content.Shared.Body.Systems;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Changeling.Systems;

public abstract partial class SharedChangelingSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] protected SharedBodySystem Body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingIdentityComponent, SwitchableOverlayToggledEvent>(OnVisionToggle);
    }

    [SubscribeLocalEvent]
    private void OnBeforeMartialArtKnowledgeAdded(Entity<ChangelingComponent> ent, ref BeforeMartialArtKnowledgeAdded args)
    {
        _popup.PopupPredicted(Loc.GetString("cqc-fail-changeling"), ent, ent);
        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnBeforeMartialArtLearningKnowledgeAdded(Entity<ChangelingComponent> ent, ref BeforeMartialArtLearningKnowledgeAdded args)
    {
        _popup.PopupPredicted(Loc.GetString("cqc-fail-changeling"), ent, ent);
        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnBeforeKravMagaAdded(Entity<ChangelingComponent> ent, ref BeforeKravMagaAdded args)
    {
        _popup.PopupPredicted(Loc.GetString("cqc-fail-changeling"), ent, ent);
        args.Cancel();
    }

    private void OnVisionToggle(Entity<ChangelingIdentityComponent> ent, ref SwitchableOverlayToggledEvent args)
    {
        if (args.User != ent.Owner)
            return;

        if (TryComp(ent, out EyeProtectionComponent? eyeProtection))
            eyeProtection.ProtectionTime = args.Activated ? TimeSpan.Zero : TimeSpan.FromSeconds(10);

        UpdateFlashImmunity(ent, !args.Activated);
    }

    protected virtual void UpdateFlashImmunity(EntityUid uid, bool active) { }
}
