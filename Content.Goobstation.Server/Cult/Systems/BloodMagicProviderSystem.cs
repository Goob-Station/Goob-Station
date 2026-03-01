using Content.Goobstation.Server.Cult.Actions;
using Content.Goobstation.Server.Cult.GameTicking;
using Content.Goobstation.Shared.Actions;
using Content.Goobstation.Shared.Cult.Events;
using Content.Goobstation.Shared.Cult.Magic;
using Content.Goobstation.Shared.UserInterface;
using Content.Server.Actions;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared.Actions.Components;
using Content.Shared.Charges.Components;
using Content.Shared.DoAfter;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Goobstation.Server.Cult.Systems;
public sealed partial class BloodMagicProviderSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _aud = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly BloodCultRuleSystem _bloodCultRule = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodMagicProviderComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<BloodMagicProviderComponent, EventActionCultPrepareBloodMagic>(OnPrepareBloodMagic);
        SubscribeLocalEvent<BloodMagicProviderComponent, EntityRadialMenuSelectMessage>(OnEntitySelect);
        SubscribeLocalEvent<BloodMagicProviderComponent, EventActionCultPrepareBloodMagicDoAfter>(OnDoAfter);
        SubscribeLocalEvent<BloodCultActionComponent, ActionRemovedFromUIControllerEvent>(OnActionRemoved);
    }

    #region Event Handlers

    private void OnPrepareBloodMagic(Entity<BloodMagicProviderComponent> ent, ref EventActionCultPrepareBloodMagic args)
    {
        if (!_bloodCultRule.TryGetRule(out var rule))
            return;

        var tiers = ent.Comp.Spells.Where(q => q.Key >= rule!.Value.Comp.CurrentTier).ToList();
        var spells = new List<EntProtoId>();
        foreach (var tier in tiers) spells.AddRange(tier.Value);

        _ui.TryOpenUi(args.Action.Owner, EntityRadialMenuKey.Key, ent);
        _ui.SetUiState(args.Action.Owner, EntityRadialMenuKey.Key, new EntityRadialMenuState(spells));
    }

    private void OnEntitySelect(Entity<BloodMagicProviderComponent> ent, ref EntityRadialMenuSelectMessage args)
    {
        _popup.PopupEntity(Loc.GetString("cult-magic-gain-start"), args.Actor, args.Actor);
        _aud.PlayPvs(new SoundCollectionSpecifier("ScalpelCut"), args.Actor, AudioParams.Default);
        var da = new DoAfterArgs(EntityManager, args.Actor, 3.5f, new EventActionCultPrepareBloodMagicDoAfter(args.ID), args.Actor, args.Actor);
        _doAfter.TryStartDoAfter(da);
    }

    private void OnDoAfter(Entity<BloodMagicProviderComponent> ent, ref EventActionCultPrepareBloodMagicDoAfter args)
    {
        GrantSpell(ent, args.SpellId, true);
    }

    private void OnActionRemoved(Entity<BloodCultActionComponent> ent, ref ActionRemovedFromUIControllerEvent args)
    {
        if (!HasComp<LimitedChargesComponent>(ent))
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
