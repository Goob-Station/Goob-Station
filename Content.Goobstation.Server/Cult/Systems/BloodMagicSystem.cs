using Content.Goobstation.Server.Cult.Actions;
using Content.Goobstation.Shared.Actions;
using Content.Goobstation.Shared.Cult.Events;
using Content.Goobstation.Shared.Cult.Magic;
using Content.Goobstation.Shared.UserInterface;
using Content.Server.Actions;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Cult.Systems;
public sealed partial class BloodMagicSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _aud = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodMagicProviderComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<BloodMagicProviderComponent, EventActionCultPrepareBloodMagic>(OnPrepareBloodMagic);
        SubscribeLocalEvent<BloodMagicProviderComponent, EntityRadialMenuSelectMessage>(OnEntitySelect);
        SubscribeLocalEvent<BloodMagicProviderComponent, EventActionCultPrepareBloodMagicDoAfter>(OnDoAfter);

        SubscribeLocalEvent<BloodCultActionComponent, ComponentStartup>(OnActionComponentStartup);
        SubscribeLocalEvent<BloodCultActionComponent, ExaminedEvent>(OnActionExamined);
        SubscribeLocalEvent<BloodCultActionComponent, ActionPerformedEvent>(OnActionPerformed);
        SubscribeLocalEvent<BloodCultActionComponent, ActionRemovedFromUIControllerEvent>(OnActionRemoved);
    }

    #region Event Handlers

    private void OnPrepareBloodMagic(Entity<BloodMagicProviderComponent> ent, ref EventActionCultPrepareBloodMagic args)
    {
        _ui.TryOpenUi(args.Action.Owner, EntityRadialMenuKey.Key, ent);
    }

    private void OnEntitySelect(Entity<BloodMagicProviderComponent> ent, ref EntityRadialMenuSelectMessage args)
    {
        _popup.PopupEntity(Loc.GetString("cult-magic-gain-start"), ent, ent);
        _aud.PlayPvs(new SoundCollectionSpecifier("ScalpelCut"), ent, AudioParams.Default);
        var da = new DoAfterArgs(EntityManager, ent, 3.5f, new EventActionCultPrepareBloodMagicDoAfter(args.ID), ent, ent);
        _doAfter.TryStartDoAfter(da);
    }

    private void OnDoAfter(Entity<BloodMagicProviderComponent> ent, ref EventActionCultPrepareBloodMagicDoAfter args)
    {
        GrantSpell(ent, args.SpellId, true);
    }

    private void OnActionComponentStartup(Entity<BloodCultActionComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.Uses = ent.Comp.MaxUses;
    }

    private void OnActionExamined(Entity<BloodCultActionComponent> ent, ref ExaminedEvent args)
    {
        using var _ = args.PushGroup(nameof(BloodCultActionComponent));
        if (!ent.Comp.UnlimitedUses)
        {
            if (ent.Comp.MaxUses > 1)
                args.PushMarkup(Loc.GetString("cult-magic-examine-uses", ("n", ent.Comp.Uses)));
            else args.PushMarkup(Loc.GetString("cult-magic-examine-uses-single"));
        }
    }

    private void OnActionPerformed(Entity<BloodCultActionComponent> ent, ref ActionPerformedEvent args)
    {
        if (!string.IsNullOrWhiteSpace(ent.Comp.InvocationLoc))

        ent.Comp.Uses = ent.Comp.UnlimitedUses ? ent.Comp.Uses : ent.Comp.Uses - 1;
        if (!ent.Comp.UnlimitedUses && ent.Comp.Uses <= 0)
            RemoveSpell(args.Performer, ent.Owner);
    }

    private void OnActionRemoved(Entity<BloodCultActionComponent> ent, ref ActionRemovedFromUIControllerEvent args)
    {
        if (ent.Comp.UnlimitedUses)
            return;

        if (!TryComp<ActionComponent>(ent, out var action) || !action.AttachedEntity.HasValue
        || !TryComp(ent, out MetaDataComponent? metadata))
            return;

        // so if a player right clicks the action it gets removed instead of being hidden.
        // it's very convenient this way imo
        var performer = action.AttachedEntity.Value;
        RemoveSpell(performer, ent.Owner, popup: true);
        _popup.PopupEntity(Loc.GetString("cult-magic-loss", ("spell", metadata.EntityName)), performer, performer);
    }

    private void OnComponentStartup(Entity<BloodMagicProviderComponent> ent, ref ComponentStartup args)
    {
        GrantSpell(ent, ent.Comp.SpellsProviderActionId, false);
    }

    #endregion

    #region API

    // todo make generic in case any more expendable spell systems are made
    public void GrantSpell(Entity<BloodMagicProviderComponent> ent, EntProtoId spellId, bool takeSlot = true)
    {
        if (_actions.TryGetActionById(ent, ent.Comp.SpellsProviderActionId, out var _))
            return;

        var spells = ent.Comp.Spells.Count;
        var maxSpells = ent.Comp.Enhanced ? ent.Comp.MaxEnhancedSpells : ent.Comp.MaxSpells;
        if (takeSlot && spells >= maxSpells)
        {
            // this
            _popup.PopupEntity(Loc.GetString($"cult-magic-limit{(ent.Comp.Enhanced ? "" : "-soft")}-reached"), ent, ent);
            return;
        }

        var spell = _actions.AddAction(ent, spellId);
        if (TryComp<BloodCultActionComponent>(spell, out var bcac))
            bcac.Enhanced = ent.Comp.Enhanced;

        if (takeSlot && TryComp(spell, out MetaDataComponent? metadata))
            _popup.PopupEntity(Loc.GetString("cult-magic-gain", ("spell", metadata.EntityName)), ent, ent);
    }

    public void RemoveSpell(Entity<BloodMagicProviderComponent?> ent, EntityUid id, bool popup = false)
    {
        _actions.RemoveAction(ent.Owner, id);

        if (popup && TryComp(id, out MetaDataComponent? metadata))
            _popup.PopupEntity(Loc.GetString("cult-magic-loss", ("spell", metadata.EntityName)), ent, ent);
    }

    public void RemoveSpell(Entity<BloodMagicProviderComponent?> ent, EntProtoId spellId, bool popup = false)
    {
        if (!_actions.TryGetActionById(ent, spellId, out var action))
            return;

        RemoveSpell(ent, action.Value, popup: popup);
    }

    #endregion
}
