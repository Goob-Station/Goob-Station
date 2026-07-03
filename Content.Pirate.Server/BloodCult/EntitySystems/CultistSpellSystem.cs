// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Content.Server.Power.Components;
using Content.Shared.FixedPoint;
using Content.Server.Popups;
using Content.Server.Hands.Systems;
using Content.Shared.Actions;
using Content.Shared.Stacks;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Prototypes;
using Content.Server.BloodCult.Components;
using Content.Shared.BloodCult.Components;
using Content.Shared.DoAfter;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Server.GameTicking.Rules;
using Content.Shared.StatusEffect;
using Content.Shared.Speech.Muting;
using Content.Shared.Stunnable;
using Content.Shared.Emp;
using Content.Server.Emp;
using Content.Shared.Popups;
using Content.Server.PowerCell;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Server.Construction;
using Content.Server.Construction.Components;
using Content.Server.Body.Systems;
using Content.Server.Body.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mind.Components;
using Content.Server.Stack;


namespace Content.Server.BloodCult.EntitySystems;

public sealed partial class CultistSpellSystem : EntitySystem
{
	private static readonly ProtoId<StackPrototype> RunedSteelStack = "RunedSteel";
	private static readonly ProtoId<StackPrototype> RunedGlassStack = "RunedGlass";
	private static readonly ProtoId<StackPrototype> RunedPlasteelStack = "RunedPlasteel";

	[Dependency] private readonly IRobustRandom _random = default!;
	[Dependency] private readonly IPrototypeManager _proto = default!;
	[Dependency] private readonly SharedActionsSystem _action = default!;
	[Dependency] private readonly DamageableSystem _damageableSystem = default!;
	[Dependency] private readonly PopupSystem _popup = default!;
	[Dependency] private readonly SharedAudioSystem _audioSystem = default!;
	[Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
	[Dependency] private readonly BloodCultRuleSystem _bloodCultRules = default!;
	[Dependency] private readonly HandsSystem _hands = default!;
	//[Dependency] private readonly StaminaSystem _stamina = default!;
	[Dependency] private readonly EmpSystem _emp = default!;
	[Dependency] private readonly PowerCellSystem _powerCell = default!;
	[Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
	[Dependency] private readonly SharedTransformSystem _transform = default!;
	[Dependency] private readonly MapSystem _mapSystem = default!;
	[Dependency] private readonly IMapManager _mapManager = default!;
	//[Dependency] private readonly IEntityManager _entMan = default!;
	[Dependency] private readonly SharedStunSystem _stun = default!;
	//[Dependency] private readonly ConstructionSystem _construction = default!;
	[Dependency] private readonly BloodstreamSystem _bloodstream = default!;
	[Dependency] private readonly StackSystem _stackSystem = default!;

	private static readonly ProtoId<DamageTypePrototype> BloodlossDamageType = "Bloodloss";
	private static readonly ProtoId<DamageTypePrototype> IonDamageType = "Ion";
	private static readonly ProtoId<DamageTypePrototype> SlashDamageType = "Slash";

	private EntityQuery<EmpowerOnStandComponent> _runeQuery;

	private static string[] AvailableDaggers = ["CultDaggerCurved", "CultDaggerSerrated", "CultDaggerStraight"];

	public override void Initialize()
	{
		base.Initialize();

		_runeQuery = GetEntityQuery<EmpowerOnStandComponent>();

		SubscribeLocalEvent<BloodCultistComponent, SpellsMessage>(OnSpellSelectedMessage);

		SubscribeLocalEvent<BloodCultistComponent, EventCultistSummonDagger>(OnSummonDagger);

		SubscribeLocalEvent<BloodCultistComponent, EventCultistStudyVeil>(OnStudyVeil);
		SubscribeLocalEvent<BloodCultistComponent, BloodCultCommuneSendMessage>(OnCommune);
		SubscribeLocalEvent<JuggernautComponent, BloodCultCommuneSendMessage>(OnJuggernautCommune);
		SubscribeLocalEvent<BloodCultistComponent, EventCultistSanguineDream>(OnSanguineDream);
		//SubscribeLocalEvent<CultMarkedComponent, AttackedEvent>(OnMarkedAttacked);

		SubscribeLocalEvent<BloodCultistComponent, EventCultistTwistedConstruction>(OnTwistedConstruction);

		SubscribeLocalEvent<BloodCultistComponent, CarveSpellDoAfterEvent>(OnCarveSpellDoAfter);
		//SubscribeLocalEvent<BloodCultistComponent, TwistedConstructionDoAfterEvent>(OnTwistedConstructionDoAfter);
	}

	private bool TryUseAbility(Entity<BloodCultistComponent> ent, BaseActionEvent args)
	{
		if (args.Handled)
            return false;
		if (!TryComp<CultistSpellComponent>(args.Action, out var actionComp))
            return false;

		// check if enough charges remain
		if (!actionComp.Infinite)
			actionComp.Charges = actionComp.Charges - 1;

		if (actionComp.Charges == 0)
		{
			_action.RemoveAction(args.Action.Owner);
			RemoveSpell(GetSpell(actionComp.AbilityId), ent.Comp);
		}

		// apply damage
		if (actionComp.HealthCost > 0 && TryComp<DamageableComponent>(ent, out var damageable))
		{
			DamageSpecifier appliedDamageSpecifier;
			if (damageable.Damage.DamageDict.ContainsKey("Bloodloss"))
				appliedDamageSpecifier = new DamageSpecifier(_proto.Index(BloodlossDamageType), FixedPoint2.New(actionComp.HealthCost));
			else if (damageable.Damage.DamageDict.ContainsKey("Ion"))
				appliedDamageSpecifier = new DamageSpecifier(_proto.Index(IonDamageType), FixedPoint2.New(actionComp.HealthCost));
			else
				appliedDamageSpecifier = new DamageSpecifier(_proto.Index(SlashDamageType), FixedPoint2.New(actionComp.HealthCost));
			_damageableSystem.TryChangeDamage(ent, appliedDamageSpecifier, true, origin: ent);
		}

		// verbalize invocation - generate random 2-word chant (skip for StudyVeil and Commune abilities)
		if (actionComp.AbilityId != "StudyVeil" && actionComp.AbilityId != "Commune")
		{
			var invocation = _bloodCultRules.GenerateChant(wordCount: 2);
			_bloodCultRules.Speak(ent, invocation);
		}

		// play sound
		if (actionComp.CastSound != null)
			_audioSystem.PlayPvs(actionComp.CastSound, ent);

		return true;
	}

	public CultAbilityPrototype GetSpell(ProtoId<CultAbilityPrototype> id)
		=> _proto.Index(id);

	public void AddSpell(EntityUid uid, BloodCultistComponent comp, ProtoId<CultAbilityPrototype> id, bool recordKnownSpell = true)
	{
		var data = GetSpell(id);

		bool standingOnRune = IsStandingOnEmpoweringRune(uid);

		if (comp.KnownSpells.Count > 3)
		{
			_popup.PopupEntity(Loc.GetString("cult-spell-exceeded"), uid, uid);
			return;
		}

		// If not on an empowering rune and they have existing spells, remove the actions matching those spells
		if (!standingOnRune && comp.KnownSpells.Count > 0)
		{
			RemoveActionsMatchingKnownSpells(uid, comp);
			// Clear KnownSpells since they can only have 0 spells when not on rune
			comp.KnownSpells.Clear();
			Dirty(uid, comp);
		}

        if (data.Event != null)
            RaiseLocalEvent(uid, (object) data.Event, true);

		if (data.ActionPrototypes == null || data.ActionPrototypes.Count <= 0)
			return;

		if (data.DoAfterLength > 0)
		{
			_popup.PopupEntity(standingOnRune ? Loc.GetString("cult-spell-carving-rune") : Loc.GetString("cult-spell-carving"), uid, uid, PopupType.MediumCaution);
			var dargs = new DoAfterArgs(EntityManager, uid, data.DoAfterLength * (standingOnRune ? 1 : 3), new CarveSpellDoAfterEvent(
				uid, data, recordKnownSpell, standingOnRune), uid
			)
			{
				BreakOnDamage = true,
				RequireCanInteract = false,  // Allow restrained cultists to prepare spells
				NeedHand = false,  // Cultists don't need hands to prep spells
				BreakOnHandChange = false,
				BreakOnMove = true,
				BreakOnDropItem = false,
				CancelDuplicate = false,
			};

			_doAfter.TryStartDoAfter(dargs);
		}
		else
		{
			foreach (var act in data.ActionPrototypes)
				_action.AddAction(uid, act);
			if (recordKnownSpell)
				comp.KnownSpells.Add(data);
		}
	}

	public void OnCarveSpellDoAfter(Entity<BloodCultistComponent> ent, ref CarveSpellDoAfterEvent args)
	{
		if (ent.Comp.KnownSpells.Count > 3 || (!args.StandingOnRune && ent.Comp.KnownSpells.Count > 0))
		{
			_popup.PopupEntity(Loc.GetString("cult-spell-exceeded"), ent, ent);
			return;
		}
		if (args.CultAbility.ActionPrototypes == null)
			return;

		DamageSpecifier appliedDamageSpecifier = new DamageSpecifier(
			_proto.Index(SlashDamageType),
			FixedPoint2.New(args.CultAbility.HealthDrain * (args.StandingOnRune ? 1 : 3))
		);

        if (!args.Cancelled)
		{
			foreach (var act in args.CultAbility.ActionPrototypes)
			{
				_action.AddAction(args.CarverUid, act);
			}
			if (args.RecordKnownSpell)
				ent.Comp.KnownSpells.Add(args.CultAbility);
			
			_damageableSystem.TryChangeDamage(ent, appliedDamageSpecifier, true, origin: ent);
			_audioSystem.PlayPvs(args.CultAbility.CarveSound, ent);
		if (args.StandingOnRune)
		{
			// Generate random chant when empowered by rune
			var invocation = _bloodCultRules.GenerateChant(wordCount: 2);
			_bloodCultRules.Speak(ent, invocation);
		}
		}

        Dirty(ent, ent.Comp);
	}

	public void RemoveSpell(ProtoId<CultAbilityPrototype> id, BloodCultistComponent comp)
	{
		comp.KnownSpells.Remove(GetSpell(id));
	}

	/// <summary>
	/// Checks if a cultist is currently standing on an EmpoweringRune.
	/// </summary>
	private bool IsStandingOnEmpoweringRune(EntityUid uid)
	{
		var coords = new EntityCoordinates(uid, default);
		var location = coords.AlignWithClosestGridTile(entityManager: EntityManager, mapManager: _mapManager);
		var gridUid = _transform.GetGrid(location);
		if (!TryComp<MapGridComponent>(gridUid, out var grid))
			return false;

		var targetTile = _mapSystem.GetTileRef(gridUid.Value, grid, location);
		foreach (var possibleEnt in _mapSystem.GetAnchoredEntities(gridUid.Value, grid, targetTile.GridIndices))
		{
			if (_runeQuery.HasComponent(possibleEnt))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Removes actions that match spells in the cultist's KnownSpells list.
	/// This is called when adding a new spell while not on an empowering rune.
	/// </summary>
	private void RemoveActionsMatchingKnownSpells(EntityUid uid, BloodCultistComponent cultist)
	{
		// Get all actions for this cultist
		var actions = _action.GetActions(uid);
		
		// Create a set of spell IDs for quick lookup
		var knownSpellIds = new HashSet<ProtoId<CultAbilityPrototype>>(cultist.KnownSpells);

		// Remove actions that match spells in KnownSpells
		foreach (var action in actions)
		{
			if (!TryComp<CultistSpellComponent>(action.Id, out var spellComp))
				continue;

			// Check if this action's AbilityId matches any spell in KnownSpells
			if (knownSpellIds.Contains(spellComp.AbilityId))
			{
				_action.RemoveAction(uid, action.Id);
			}
		}
	}

	private void OnStudyVeil(Entity<BloodCultistComponent> ent, ref EventCultistStudyVeil args)
	{
		if (!TryUseAbility(ent, args))
			return;

		ent.Comp.StudyingVeil = true;
		args.Handled = true;
	}

	private void OnCommune(Entity<BloodCultistComponent> ent, ref BloodCultCommuneSendMessage args)
	{
		ent.Comp.CommuningMessage = args.Message;
	}

	private void OnJuggernautCommune(Entity<JuggernautComponent> ent, ref BloodCultCommuneSendMessage args)
	{
		// Source-level fall-through: if this entity also holds BloodCultistComponent, the
		// cultist handler already stamped the message and the rule's Update loop will
		// dispatch it exactly once. Stamping here as well would queue a second
		// DistributeCommune call (one per component) for a single user action.
		if (HasComp<BloodCultistComponent>(ent))
			return;

		ent.Comp.CommuningMessage = args.Message;
	}

	private void OnSpellSelectedMessage(Entity<BloodCultistComponent> ent, ref SpellsMessage args)
	{
		if (!CultistSpellComponent.ValidSpells.Contains(args.ProtoId) || ent.Comp.KnownSpells.Contains(args.ProtoId))
		{
			_popup.PopupEntity(Loc.GetString("cult-spell-havealready"), ent, ent);
			return;
		}
		AddSpell(ent, ent.Comp, args.ProtoId, recordKnownSpell:true);
	}

	private void OnSummonDagger(Entity<BloodCultistComponent> ent, ref EventCultistSummonDagger args)
	{
		if (!TryUseAbility(ent, args))
			return;

		var dagger = Spawn(_random.Pick(AvailableDaggers), Transform(ent).Coordinates);
		if (!_hands.TryForcePickupAnyHand(ent, dagger))
		{
			_popup.PopupEntity(Loc.GetString("cult-spell-fail"), ent, ent);
			QueueDel(dagger);
			return;
		}

		args.Handled = true;
	}

	/// <summary>
	/// Helper method to check if an entity is a cultist, including SSD cultists by checking their mind.
	/// </summary>
	private bool IsTargetCultist(EntityUid target)
	{
		// Check if the target's body has BloodCultistComponent
		if (HasComp<BloodCultistComponent>(target))
			return true;

		// Check if the target's mind has BloodCultistComponent (for SSD cultists)
		if (TryComp<MindContainerComponent>(target, out var mindContainer) && 
		    mindContainer.Mind != null &&
		    HasComp<BloodCultistComponent>(mindContainer.Mind.Value))
			return true;

		return false;
	}

	private void OnSanguineDream(Entity<BloodCultistComponent> ent, ref EventCultistSanguineDream args)
	{
		if (args.Handled)
			return;

		var target = args.Target;

		// Check if target is an allied cultist (including SSD cultists)
		if (IsTargetCultist(target))
		{
			_popup.PopupEntity(
				Loc.GetString("cult-spell-allied-cultist"),
				ent, ent, PopupType.MediumCaution
			);
			return;
		}

		if (!TryUseAbility(ent, args))
			return;

		args.Handled = true;

		// Mindshield protects from nocturine injection, but still stuns briefly
		if (HasComp<MindShieldComponent>(target))
		{
			// Stun the target briefly - this will make them drop prone and drop items
			_stun.TryKnockdown(target, TimeSpan.FromSeconds(3), true);
			return;
		}

		float empDamage = 5000f;  // EMP damage for borgs/IPCs
		float empDuration = 12f;  // EMP duration in seconds
		int selfStunTime = 4;

		// Holy protection repels cult magic
		if (HasComp<CultResistantComponent>(target))
		{
			_popup.PopupEntity(
					Loc.GetString("cult-spell-repelled"),
					ent, ent, PopupType.MediumCaution
				);
			_audioSystem.PlayPvs(new SoundPathSpecifier("/Audio/Effects/holy.ogg"), Transform(ent).Coordinates);
			// Knock down the cultist who cast the spell. Might need balancing
			_stun.TryKnockdown(ent, TimeSpan.FromSeconds(selfStunTime), true);
		}
		else if (HasComp<SiliconComponent>(target) &&
			_powerCell.TryGetBatteryFromSlot(target, out EntityUid? ipcBatteryUid, out BatteryComponent? _) &&
			ipcBatteryUid != null)
		{
			// Apply EMP damage directly to the IPC's battery
			_emp.DoEmpEffects((EntityUid)ipcBatteryUid, empDamage, empDuration);
			_statusEffect.TryAddStatusEffect<MutedComponent>(target, "Muted", TimeSpan.FromSeconds(empDuration), false);
		}
		else if (HasComp<BorgChassisComponent>(target) &&
			_powerCell.TryGetBatteryFromSlot(target, out EntityUid? borgBatteryUid, out BatteryComponent? _) &&
			borgBatteryUid != null)
		{
			// Apply EMP damage directly to the borg's battery
			_emp.DoEmpEffects((EntityUid)borgBatteryUid, empDamage, empDuration);
			_statusEffect.TryAddStatusEffect<MutedComponent>(target, "Muted", TimeSpan.FromSeconds(empDuration), false);
		}
		else if (HasComp<JuggernautComponent>(target))
		{
			// Juggernauts are immune to sanguine dream (they have no bloodstream)
			_popup.PopupEntity(
				Loc.GetString("cult-spell-fail"),
				ent, ent, PopupType.MediumCaution
			);
		}
		else if (TryComp<BloodstreamComponent>(target, out var bloodstream))
		{
			// Inject sleep chemicals (Nocturine + Chloral Hydrate)
			var sleepSolution = new Solution();
			sleepSolution.AddReagent("Nocturine", FixedPoint2.New(15));  // 15u Nocturine
			sleepSolution.AddReagent("EdgeEssentia", FixedPoint2.New(5));  // 5u Edge Essentia
			
			_bloodstream.TryAddToChemicals(target, sleepSolution, bloodstream);
			
			// Show the dream message
			_popup.PopupEntity(
				Loc.GetString("cult-spell-sleep-dream"),
				target, target, PopupType.LargeCaution
			);
			
			// Mark them for follow-up attacks
			// disabled for now, follow up attacks work, but end up being too fancy and not really needed.
			//EnsureComp<CultMarkedComponent>(target);
			
			// Added a manual mute, since I know upstream has a possible Nocturine debuff that makes it take effect slower.
			// The intent is for this to work to kidnap any non-mindshielded crew member.
			_statusEffect.TryAddStatusEffect<MutedComponent>(target, "Muted", TimeSpan.FromSeconds(15), false);
		}
		else
		{
			// Fallback for entities without bloodstream (IPCs, rare cases)
			// Apply EMP effects directly to the entity, and mute them.
			// todo: Explore if this is a less fancy way to do it. Might be able to just do this for all entities?
			_emp.DoEmpEffects(target, empDamage, empDuration);
			_statusEffect.TryAddStatusEffect<MutedComponent>(target, "Muted", TimeSpan.FromSeconds(empDuration), false);
		}
	}

	// Disabled for now. May be re-enabled if balance needs it.
	//private void OnMarkedAttacked(Entity<CultMarkedComponent> ent, ref AttackedEvent args)
	//{
	//	var advancedStaminaDamage = 100;
	//	var advancedStunTime = 15;
	//	if (HasComp<BloodCultRuneCarverComponent>(args.Used))
	//	{
	//		_stun.TryKnockdown(ent, TimeSpan.FromSeconds(advancedStunTime), true);
	//		_stamina.TakeStaminaDamage(ent, advancedStaminaDamage, visual: false);
	//		_stun.TryStun(ent, TimeSpan.FromSeconds(advancedStunTime), true);
	//		_statusEffect.TryAddStatusEffect<MutedComponent>(ent, "Muted", TimeSpan.FromSeconds(advancedStunTime), false);
	//		_entMan.RemoveComponent<CultMarkedComponent>(ent);
	//		_audioSystem.PlayPvs(new SoundPathSpecifier("/Audio/Items/Defib/defib_zap.ogg"), ent, AudioParams.Default.WithVolume(-3f));
	//	}
	//}


	private void OnTwistedConstruction(Entity<BloodCultistComponent> ent, ref EventCultistTwistedConstruction args)
	{
		// Check if target is a steel stack
		if (TryComp<StackComponent>(args.Target, out var stack) && stack.StackTypeId == "Steel")
		{
			if (!TryUseAbility(ent, args))
				return;

			var count = stack.Count;
			var targetCoords = Transform(args.Target).Coordinates;

			// Use StackSystem.SpawnMultiple to properly split stacks respecting max count (30)
			var runedSteelProto = _proto.Index(RunedSteelStack);
			_stackSystem.SpawnMultiple(runedSteelProto.Spawn.ToString(), count, targetCoords);

			QueueDel(args.Target);
			args.Handled = true;
			return;
		}

		// Check if target is a glass stack
		if (TryComp<StackComponent>(args.Target, out var glassStack) && glassStack.StackTypeId == "Glass")
		{
			if (!TryUseAbility(ent, args))
				return;

			var count = glassStack.Count;
			var targetCoords = Transform(args.Target).Coordinates;

			// Use StackSystem.SpawnMultiple to properly split stacks respecting max count (30)
			var runedGlassProto = _proto.Index(RunedGlassStack);
			_stackSystem.SpawnMultiple(runedGlassProto.Spawn.ToString(), count, targetCoords);

			QueueDel(args.Target);
			args.Handled = true;
			return;
		}

		// Check if target is a reinforced glass stack
		if (TryComp<StackComponent>(args.Target, out var reinforcedGlassStack) && reinforcedGlassStack.StackTypeId == "ReinforcedGlass")
		{
			if (!TryUseAbility(ent, args))
				return;

			var count = reinforcedGlassStack.Count;
			var targetCoords = Transform(args.Target).Coordinates;

			// Use StackSystem.SpawnMultiple to properly split stacks respecting max count (30)
			var runedGlassProto = _proto.Index(RunedGlassStack);
			_stackSystem.SpawnMultiple(runedGlassProto.Spawn.ToString(), count, targetCoords);

			QueueDel(args.Target);
			args.Handled = true;
			return;
		}

		// Check if target is a plasteel stack
		if (TryComp<StackComponent>(args.Target, out var plasteelStack) && plasteelStack.StackTypeId == "Plasteel")
		{
			if (!TryUseAbility(ent, args))
				return;

			var count = plasteelStack.Count;
			var targetCoords = Transform(args.Target).Coordinates;

			// Use StackSystem.SpawnMultiple to properly split stacks respecting max count (30)
			var runedPlasteelProto = _proto.Index(RunedPlasteelStack);
			_stackSystem.SpawnMultiple(runedPlasteelProto.Spawn.ToString(), count, targetCoords);

			QueueDel(args.Target);
			args.Handled = true;
			return;
		}

		// Check if target is a reinforced wall
		/*if (TryComp<ConstructionComponent>(args.Target, out var construction) && 
		    construction.Graph == "Girder" && 
		    construction.Node == "reinforcedWall")
		{
			// Wall deconstruction doesn't consume spell charges
			// Generate random chant for wall deconstruction
			if (TryComp<CultistSpellComponent>(args.Action, out var actionComp))
			{
				var invocation = _bloodCultRules.GenerateChant(wordCount: 2);
				_bloodCultRules.Speak(ent, invocation);
				if (actionComp.CastSound != null)
					_audioSystem.PlayPvs(actionComp.CastSound, ent);
			}

			// Start do-after for wall deconstruction
			var doAfterArgs = new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(3), 
				new TwistedConstructionDoAfterEvent(args.Target), ent, target: args.Target)
			{
				BreakOnMove = true,
				BreakOnDamage = true,
				NeedHand = false,
			};

			_doAfter.TryStartDoAfter(doAfterArgs);
			args.Handled = true;
			return;
		}*/

		// Check if target is a reinforced girder
		/*if (TryComp<ConstructionComponent>(args.Target, out var girderConstruction) && 
		    girderConstruction.Graph == "Girder" && 
		    girderConstruction.Node == "reinforcedGirder")
		{
			// Girder downgrade doesn't consume spell charges
			// Generate random chant for girder deconstruction
			if (TryComp<CultistSpellComponent>(args.Action, out var actionComp))
			{
				var invocation = _bloodCultRules.GenerateChant(wordCount: 2);
				_bloodCultRules.Speak(ent, invocation);
				if (actionComp.CastSound != null)
					_audioSystem.PlayPvs(actionComp.CastSound, ent);
			}

			// Start do-after for reinforced girder downgrade
			var doAfterArgs = new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(2), 
				new TwistedConstructionDoAfterEvent(args.Target), ent, target: args.Target)
			{
				BreakOnMove = true,
				BreakOnDamage = true,
				NeedHand = false,
			};

			_doAfter.TryStartDoAfter(doAfterArgs);
			args.Handled = true;
			return;
		}*/
	}

	/*private void OnTwistedConstructionDoAfter(Entity<BloodCultistComponent> ent, ref TwistedConstructionDoAfterEvent args)
	{
		if (args.Cancelled || !TryComp<ConstructionComponent>(args.Target, out var construction))
			return;

		// Verify it's a valid target (reinforced wall or reinforced girder)
		if (construction.Graph != "Girder" || 
		    (construction.Node != "reinforcedWall" && construction.Node != "reinforcedGirder"))
			return;

		// Don't consume spell charges for wall deconstruction - only for plasteel conversion
		var targetCoords = Transform(args.Target).Coordinates;

		if (construction.Node == "reinforcedWall")
		{
			// Reinforced wall -> reinforced girder
			// Spawn a stack of 2 plasteel sheets (the amount to upgrade girder to reinforced girder)
			// Use StackSystem.Spawn to properly initialize the stack for client-side rendering
			_stackSystem.Spawn(2, new ProtoId<StackPrototype>("Plasteel"), targetCoords);

			// Change the wall to a reinforced girder (this will spawn the 2 plasteel from the wall plating)
			// The construction graph automatically handles spawning materials when deconstructing
			_construction.ChangeNode(args.Target, ent, "reinforcedGirder", performActions: true, construction: construction);
		}
		else if (construction.Node == "reinforcedGirder")
		{
			// Reinforced girder -> regular girder
			// Spawn a stack of 2 plasteel sheets (the amount used to upgrade to reinforced girder)
			// Use StackSystem.Spawn to properly initialize the stack for client-side rendering
			_stackSystem.Spawn(2, new ProtoId<StackPrototype>("Plasteel"), targetCoords);

			// Change the reinforced girder to a regular girder
			_construction.ChangeNode(args.Target, ent, "girder", performActions: true, construction: construction);
		}
	}*/
}
