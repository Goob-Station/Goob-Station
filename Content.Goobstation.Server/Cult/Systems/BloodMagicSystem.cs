using Content.Goobstation.Server.Cult.Actions;
using Content.Goobstation.Shared.Cult.Events;
using Content.Goobstation.Shared.Cult.Magic;
using Content.Goobstation.Shared.UserInterface;
using Content.Server.Actions;
using Content.Server.DoAfter;
using Content.Shared.Actions.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Cult.Systems;
public sealed partial class BloodMagicSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _aud = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodMagicProviderComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<BloodMagicProviderComponent, EventActionCultPrepareBloodMagic>(OnPrepareBloodMagic);
        SubscribeLocalEvent<BloodMagicProviderComponent, EntityRadialMenuSelectMessage>(OnEntitySelect);

        // todo uses description view
        SubscribeLocalEvent<BloodCultActionComponent, ActionPerformedEvent>(OnActionPerformed);
    }

    private void OnPrepareBloodMagic(Entity<BloodMagicProviderComponent> ent, ref EventActionCultPrepareBloodMagic args)
    {
        _ui.TryOpenUi(args.Action.Owner, EntityRadialMenuKey.Key, ent);
    }

    private void OnEntitySelect(Entity<BloodMagicProviderComponent> ent, ref EntityRadialMenuSelectMessage args)
    {
        TryGrantSpell(ent, args.ID, true);
    }

    private void OnActionPerformed(Entity<BloodCultActionComponent> ent, ref ActionPerformedEvent args)
    {
        // todo damage, effects, etc

        ent.Comp.Uses = ent.Comp.UnlimitedUses ? ent.Comp.Uses : ent.Comp.Uses - 1;
        if (!ent.Comp.UnlimitedUses && ent.Comp.Uses <= 0)
            _actions.RemoveAction(args.Performer, ent.Owner);
    }

    private void OnComponentStartup(Entity<BloodMagicProviderComponent> ent, ref ComponentStartup args)
    {
        TryGrantSpell(ent, ent.Comp.SpellsProviderActionId, false);
    }

    // todo make generic in case any more expendable spell systems are made
    public bool TryGrantSpell(Entity<BloodMagicProviderComponent> ent, EntProtoId spellId, bool takeSlot = true)
    {
        if (_actions.TryGetActionById(ent, ent.Comp.SpellsProviderActionId, out _))
            return true;

        if (takeSlot)
        {
            var maxSpells = ent.Comp.Enhanced ? ent.Comp.MaxEnhancedSpells : ent.Comp.MaxSpells;
            if (ent.Comp.Spells.Count >= maxSpells)
                return false;
        }

        var spell = _actions.AddAction(ent, spellId);
        if (TryComp<BloodCultActionComponent>(spell, out var bcac))
            bcac.Enhanced = ent.Comp.Enhanced;

        return true;
    }

    public bool TryRemoveSpell(Entity<BloodMagicProviderComponent> ent, EntProtoId spellId)
    {
        if (!_actions.TryGetActionById(ent, spellId, out var action))
            return true;

        _actions.RemoveAction(ent.Owner, action!);
        return true;
    }
}
