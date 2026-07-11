// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2026 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using System.Linq;
using Robust.Shared.GameObjects;
using Content.Server.Ghost;
using Content.Server.Mind;
using Content.Server.NPC.Systems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.DragDrop;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Server.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Content.Shared.Damage;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;
using Content.Shared.Speech;
using Content.Shared.Emoting;
using Robust.Shared.Map;
using Content.Shared.NPC.Systems;
using Content.Shared.NPC.Components;
using Content.Server.GameTicking.Rules;
using Content.Shared.Actions;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Player;
using Content.Shared.Damage.Components;
using Content.Shared._White.RadialSelector;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.BloodCult.EntitySystems;

public sealed partial class BloodCultConstructSystem : EntitySystem
{
	[Dependency] private readonly MindSystem _mind = default!;
	[Dependency] private readonly GhostSystem _ghost = default!;
	[Dependency] private readonly NPCSystem _npc = default!;
	[Dependency] private readonly MobStateSystem _mobState = default!;
	[Dependency] private readonly PopupSystem _popup = default!;
	[Dependency] private readonly SharedAudioSystem _audio = default!;
	[Dependency] private readonly SharedContainerSystem _container = default!;
	[Dependency] private readonly SharedPhysicsSystem _physics = default!;
	[Dependency] private readonly IRobustRandom _random = default!;
	[Dependency] private readonly SharedTransformSystem _transform = default!;
	[Dependency] private readonly NpcFactionSystem _npcFaction = default!;
	[Dependency] private readonly SharedActionsSystem _actions = default!;
	[Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;
	[Dependency] private readonly UserInterfaceSystem _ui = default!;
	[Dependency] private readonly IPrototypeManager _prototype = default!;

	private readonly Dictionary<EntityUid, PendingConstructSource> _pendingConstructs = new();

	/// <summary>
	/// Grants the Commune action to a juggernaut
	/// </summary>
	private void GrantCommuneAction(EntityUid juggernaut)
	{
		EntityUid? communeAction = null;
		_actions.AddAction(juggernaut, ref communeAction, "ActionCultistCommune");
	}


	public override void Initialize()
	{
		base.Initialize();
		
		// CanDropTargetEvent is handled in SharedBloodCultistSystem for both client and server
		SubscribeLocalEvent<BloodCultConstructShellComponent, DragDropTargetEvent>(OnDragDropTarget);
		SubscribeLocalEvent<BloodCultConstructShellComponent, RadialSelectorSelectedMessage>(OnConstructSelected);
		SubscribeLocalEvent<BloodCultConstructShellComponent, ComponentShutdown>(OnShellShutdown);
		SubscribeLocalEvent<BloodCultConstructComponent, MobStateChangedEvent>(OnConstructStateChanged);
		SubscribeLocalEvent<BloodCultConstructComponent, PlayerDetachedEvent>(OnPlayerDetached);
		SubscribeLocalEvent<BloodCultConstructComponent, EntityTerminatingEvent>(OnConstructTerminating,
			before: [typeof(MindSystem)]);
		SubscribeLocalEvent<GhostAttemptHandleEvent>(OnGhostAttempt);
		SubscribeLocalEvent<JuggernautComponent, DragDropTargetEvent>(OnJuggernautDragDropTarget);
		
		// Remove StaminaComponent from any existing juggernauts (in case they were spawned before this system was added)
		// Juggernauts can't be stunned, so stamina damage is meaningless
		var query = AllEntityQuery<JuggernautComponent, StaminaComponent>();
		while (query.MoveNext(out var uid, out _, out _))
		{
			RemComp<StaminaComponent>(uid);
		}
		
		// Handle alt-fire (right-click) attack to find nearest enemy for juggernauts and shades
		// With AltDisarm = false, right-clicking sends HeavyAttackEvent instead of DisarmAttackEvent
		SubscribeNetworkEvent<HeavyAttackEvent>(OnHeavyAttack, before: new[] { typeof(SharedMeleeWeaponSystem) });
	}

	public void TryApplySoulStone(Entity<SoulStoneComponent> ent, ref AfterInteractEvent args)
    {
		if (args.Target == null)
			return;

		// Lesser shells let the cultist choose a construct form.
		if (TryComp<BloodCultConstructShellComponent>(args.Target, out var shell) && shell.Constructs.Count > 0)
		{
			BeginConstructSelection(args.Target.Value, shell, args.User, ent, BloodCultConstructSourceKind.SoulStone);
			args.Handled = true;
			return;
		}

		// The dedicated shell always creates the Pirate juggernaut.
		if (HasComp<BloodCultConstructShellComponent>(args.Target))
		{
			_ActivateJuggernautShell(ent, args.User, args.Target.Value);
			args.Handled = true;
			return;
		}

		// Check if target is an inactive juggernaut (critical state)
		if (TryComp<JuggernautComponent>(args.Target, out var juggComp) && juggComp.IsInactive)
		{
			_ReactivateJuggernaut(ent, args.User, args.Target.Value, juggComp);
			args.Handled = true;
			return;
		}
	}

	private void _ActivateJuggernautShell(EntityUid soulstone, EntityUid user, EntityUid shell)
	{
		// Get the mind from the soulstone
		EntityUid? mindId = CompOrNull<MindContainerComponent>(soulstone)?.Mind;
		MindComponent? mindComp = CompOrNull<MindComponent>(mindId);
		
		if (mindId == null || mindComp == null)
		{
			//No mind in the soulstone
			_popup.PopupEntity(Loc.GetString("cult-soulstone-empty"), user, user, PopupType.Medium);
			return;
		}
		
		// Figure out the shell's location so we can spawn the completed juggernaut there
		var shellTransform = Transform(shell);
		var shellMapCoords = _transform.GetMapCoordinates(shellTransform);
		var shellRotation = shellTransform.LocalRotation;
		
		// Unanchor the shell first to prevent grid snapping issues
		if (shellTransform.Anchored)
		{
			_transform.Unanchor(shell, shellTransform);
		}
		
		// Play sacrifice audio
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/disintegrate.ogg"), shellTransform.Coordinates);
		
		// Delete the shell and spawn the juggernaut at the exact map coordinates with rotation
		// Use DeleteEntity instead of QueueDel to ensure immediate deletion before spawning
		EntityManager.DeleteEntity(shell);
		
		// Spawn the juggernaut at the exact map coordinates (not anchored, so it won't snap to grid)
		var juggernaut = Spawn("MobBloodCultJuggernaut", shellMapCoords, rotation: shellRotation);
		
		// Remove StaminaComponent - juggernauts can't be stunned so stamina damage is meaningless
		RemComp<StaminaComponent>(juggernaut);
		
		// Ensure the juggernaut is not anchored (mobs shouldn't be anchored)
		var juggernautTransform = Transform(juggernaut);
		if (juggernautTransform.Anchored)
		{
			_transform.Unanchor(juggernaut, juggernautTransform);
		}
		
		// Store the soul stone inside the construct so the mind has somewhere to return.
		if (!TryComp<BloodCultConstructComponent>(juggernaut, out var construct) ||
			!TrySetConstructSource((juggernaut, construct), soulstone, BloodCultConstructSourceKind.SoulStone,
				"juggernaut_soulstone_container"))
		{
			QueueDel(juggernaut);
			return;
		}

		if (TryComp<JuggernautComponent>(juggernaut, out var juggComp))
		{
			juggComp.IsInactive = false;
		}
		
		// Transfer mind from soulstone to juggernaut
		_mind.TransferTo((EntityUid)mindId, juggernaut, ghostCheckOverride: true, mind: mindComp);
		
		// Preserve speech component from soulstone only if it's a Hamlet soulstone
		if (TryComp<SoulStoneComponent>(soulstone, out var soulstoneComp) && 
		    soulstoneComp.OriginalEntityPrototype == "MobHamsterHamlet" &&
		    TryComp<SpeechComponent>(soulstone, out var soulstoneSpeech))
		{
			// Remove existing speech component if present, then copy from soulstone
			if (HasComp<SpeechComponent>(juggernaut))
				RemComp<SpeechComponent>(juggernaut);
			CopyComp(soulstone, juggernaut, soulstoneSpeech);
		}
		
		// Ensure juggernaut is in the BloodCultist faction (remove any crew alignment)
		// Use ClearFactions and AddFaction to ensure proper faction alignment after mind transfer
		if (TryComp<NpcFactionMemberComponent>(juggernaut, out var npcFaction))
		{
			_npcFaction.ClearFactions((juggernaut, npcFaction), false);
		}
		_npcFaction.AddFaction(juggernaut, BloodCultRuleSystem.BloodCultistFactionId);
		
		// Grant Commune ability to juggernaut
		GrantCommuneAction(juggernaut);
		
		// Play transformation audio
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/blink.ogg"), shellTransform.Coordinates);
		
		// Play a message
		_popup.PopupEntity(Loc.GetString("cult-juggernaut-created"), user, user, PopupType.Large);
	}

	private void _ReactivateJuggernaut(EntityUid soulstone, EntityUid user, EntityUid juggernaut, JuggernautComponent juggComp)
	{
		// Get the mind from the soulstone
		EntityUid? mindId = CompOrNull<MindContainerComponent>(soulstone)?.Mind;
		MindComponent? mindComp = CompOrNull<MindComponent>(mindId);
		
		if (mindId == null || mindComp == null)
		{
			_popup.PopupEntity(Loc.GetString("cult-soulstone-empty"), user, user, PopupType.Medium);
			return;
		}

		if (!TryComp<BloodCultConstructComponent>(juggernaut, out var construct) ||
			!TrySetConstructSource((juggernaut, construct), soulstone, BloodCultConstructSourceKind.SoulStone,
				"juggernaut_soulstone_container"))
			return;

		// Reactivate the juggernaut.
		juggComp.IsInactive = false;

		// Grant Commune ability to juggernaut if not already granted
		GrantCommuneAction(juggernaut);

		// DON'T heal the juggernaut - it stays in critical state until healed with blood

		// Transfer mind from soulstone to juggernaut
		_mind.TransferTo((EntityUid)mindId, juggernaut, ghostCheckOverride: true, mind: mindComp);
		
		// Preserve speech component from soulstone (e.g., Hamlet's squeak sounds)
		if (TryComp<SpeechComponent>(soulstone, out var soulstoneSpeech))
		{
			// Remove existing speech component if present, then copy from soulstone
			if (HasComp<SpeechComponent>(juggernaut))
				RemComp<SpeechComponent>(juggernaut);
			CopyComp(soulstone, juggernaut, soulstoneSpeech);
		}
		
		// Ensure juggernaut is in the BloodCultist faction (remove any crew alignment)
		// Use ClearFactions and AddFaction to ensure proper faction alignment after mind transfer
		if (TryComp<NpcFactionMemberComponent>(juggernaut, out var npcFaction))
		{
			_npcFaction.ClearFactions((juggernaut, npcFaction), false);
		}
		_npcFaction.AddFaction(juggernaut, BloodCultRuleSystem.BloodCultistFactionId);

		// Play transformation audio
		var coordinates = Transform(juggernaut).Coordinates;
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/blink.ogg"), coordinates);

		// Notify the user
		_popup.PopupEntity(Loc.GetString("cult-juggernaut-reactivated"), user, user, PopupType.Large);
	}

	// Handle dragging dead bodies onto the juggernaut shell to create a juggernaut
	private void OnDragDropTarget(EntityUid uid, BloodCultConstructShellComponent component, ref DragDropTargetEvent args)
	{
		// Mark as handled immediately to prevent other systems from processing this
		args.Handled = true;
		
		// Verify the dragged entity is a dead body with a mind
		if (!_mobState.IsDead(args.Dragged))
		{
			_popup.PopupEntity(Loc.GetString("cult-juggernaut-shell-needs-dead"), args.User, args.User, PopupType.Medium);
			return;
		}

		EntityUid? mindId = CompOrNull<MindContainerComponent>(args.Dragged)?.Mind;
		MindComponent? mindComp = CompOrNull<MindComponent>(mindId);
		
		if (mindId == null || mindComp == null)
		{
			_popup.PopupEntity(Loc.GetString("cult-invocation-fail-nosoul"), args.User, args.User, PopupType.Medium);
			return;
		}

		if (component.Constructs.Count > 0)
		{
			BeginConstructSelection(uid, component, args.User, args.Dragged, BloodCultConstructSourceKind.Body);
			return;
		}

		var shellTransform = Transform(uid);
		var shellMapCoords = _transform.GetMapCoordinates(shellTransform);
		var shellRotation = shellTransform.LocalRotation;
		
		// Unanchor the shell first to prevent grid snapping issues
		if (shellTransform.Anchored)
		{
			_transform.Unanchor(uid, shellTransform);
		}
		
		// Spawn the juggernaut BEFORE deleting the shell to ensure proper setup
		// Spawn at the exact map coordinates (not anchored, so it won't snap to grid)
		var juggernaut = Spawn("MobBloodCultJuggernaut", shellMapCoords, rotation: shellRotation);
		
		// Remove StaminaComponent - juggernauts can't be stunned so stamina damage is meaningless
		RemComp<StaminaComponent>(juggernaut);
		
		// Ensure the juggernaut is not anchored (mobs shouldn't be anchored)
		var juggernautTransform = Transform(juggernaut);
		if (juggernautTransform.Anchored)
		{
			_transform.Unanchor(juggernaut, juggernautTransform);
		}
		
		if (!TryComp<BloodCultConstructComponent>(juggernaut, out var construct) ||
			!TrySetConstructSource((juggernaut, construct), args.Dragged, BloodCultConstructSourceKind.Body,
				"juggernaut_body_container"))
		{
			QueueDel(juggernaut);
			return;
		}
		
		// Play sacrifice audio
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/disintegrate.ogg"), shellTransform.Coordinates);
		
		// Delete the shell AFTER the body is safely in the container
		// Use DeleteEntity instead of QueueDel to ensure immediate deletion
		EntityManager.DeleteEntity(uid);
		
		// Mark the new juggernaut active.
		if (TryComp<JuggernautComponent>(juggernaut, out var juggComp))
		{
			juggComp.IsInactive = false;
		}
		
		// Transfer mind from victim to juggernaut
		_mind.TransferTo((EntityUid)mindId, juggernaut, ghostCheckOverride: true, mind: mindComp);
		
		// Ensure juggernaut is in the BloodCultist faction (remove any crew alignment)
		// Use ClearFactions and AddFaction to ensure proper faction alignment after mind transfer
		if (TryComp<NpcFactionMemberComponent>(juggernaut, out var npcFaction))
		{
			_npcFaction.ClearFactions((juggernaut, npcFaction), false);
		}
		_npcFaction.AddFaction(juggernaut, BloodCultRuleSystem.BloodCultistFactionId);
		
		// Grant Commune ability to juggernaut
		GrantCommuneAction(juggernaut);
		
		// Play transformation audio
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/blink.ogg"), shellTransform.Coordinates);
		
		// Notify the user
		_popup.PopupEntity(Loc.GetString("cult-juggernaut-created"), args.User, args.User, PopupType.Large);
		
		args.Handled = true;
	}


	private void OnConstructStateChanged(Entity<BloodCultConstructComponent> construct, ref MobStateChangedEvent args)
	{
		if (args.NewMobState == MobState.Dead && HasComp<JuggernautComponent>(construct))
			DisableProjectileCollision(construct);

		if (args.NewMobState != MobState.Dead &&
			(args.NewMobState != MobState.Critical || !construct.Comp.EjectSourceOnCritical))
			return;

		EjectConstructSource(construct);

		if (args.NewMobState == MobState.Dead && HasComp<ShadeComponent>(construct))
			QueueDel(construct);
	}

	private void OnGhostAttempt(GhostAttemptHandleEvent args)
	{
		if (args.Handled || args.Mind.VisitingEntity != null || args.Mind.OwnedEntity is not { } construct ||
			!TryComp<BloodCultConstructComponent>(construct, out var constructComp) ||
			constructComp.SourceEntity is not { } source || !Exists(source) ||
			!TryComp<MindContainerComponent>(construct, out var mindContainer) ||
			mindContainer.Mind is not { } mindId || mindId != args.Mind.Owner)
			return;

		var ghost = _ghost.SpawnGhost((mindId, args.Mind), construct, canReturn: true);
		if (ghost == null)
			return;

		_npc.WakeNPC(construct);
		args.Result = true;
		args.Handled = true;
	}

	private void OnPlayerDetached(Entity<BloodCultConstructComponent> construct, ref PlayerDetachedEvent args)
	{
		if (construct.Comp.SourceEntity != null && !_mobState.IsIncapacitated(construct))
			_npc.WakeNPC(construct);
	}

	private void OnConstructTerminating(Entity<BloodCultConstructComponent> construct, ref EntityTerminatingEvent args)
	{
		EjectConstructSource(construct);
	}

	private void EjectConstructSource(Entity<BloodCultConstructComponent> construct)
	{
		if (construct.Comp.SourceEntity is not { } source)
			return;

		var sourceKind = construct.Comp.SourceKind;
		var containerId = construct.Comp.SourceContainerId;
		construct.Comp.SourceEntity = null;
		construct.Comp.SourceContainerId = null;

		if (!Exists(source))
			return;

		var coordinates = Transform(construct).Coordinates;
		var removedFromContainer = containerId != null &&
			_container.TryGetContainer(construct, containerId, out var container) &&
			_container.Remove(source, container, destination: coordinates);

		if (removedFromContainer && TryComp<PhysicsComponent>(source, out var physics))
		{
			_physics.SetAwake((source, physics), true);
			var impulse = _random.NextVector2() * _random.NextFloat(8f, 15f) * physics.Mass;
			_physics.ApplyLinearImpulse(source, impulse, body: physics);
		}

		var mindId = CompOrNull<MindContainerComponent>(construct)?.Mind;
		var transferredMind = false;
		if (mindId != null && TryComp<MindComponent>(mindId, out var mind))
		{
			_mind.TransferTo(mindId.Value, source, mind: mind);
			transferredMind = true;

			if (sourceKind == BloodCultConstructSourceKind.Body &&
				mind.VisitingEntity == null &&
				_mobState.IsDead(source))
				_ghost.SpawnGhost((mindId.Value, mind), source, canReturn: true);
		}

		if (sourceKind == BloodCultConstructSourceKind.SoulStone)
		{
			EnsureComp<SpeechComponent>(source);
			EnsureComp<EmotingComponent>(source);
		}

		if (TryComp<JuggernautComponent>(construct, out var juggernaut))
			juggernaut.IsInactive = true;

		if (!transferredMind)
			return;

		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/blink.ogg"), coordinates);
		_popup.PopupEntity(
			Loc.GetString(sourceKind switch
			{
				BloodCultConstructSourceKind.Body => "cult-construct-body-ejected",
				_ when HasComp<JuggernautComponent>(construct) => "cult-juggernaut-critical-soulstone-ejected",
				_ => "cult-construct-soulstone-returned",
			}),
			construct,
			PopupType.LargeCaution);
	}

	private void DisableProjectileCollision(EntityUid uid)
	{
		if (TryComp<PhysicsComponent>(uid, out var physics))
			_physics.SetCanCollide(uid, false, body: physics);
	}

	private void BeginConstructSelection(
		EntityUid shell,
		BloodCultConstructShellComponent component,
		EntityUid user,
		EntityUid source,
		BloodCultConstructSourceKind sourceKind)
	{
		if (!TryGetSourceMind(source, out _, out _))
		{
			_popup.PopupEntity(Loc.GetString("cult-invocation-fail-nosoul"), user, user, PopupType.Medium);
			return;
		}

		if (sourceKind == BloodCultConstructSourceKind.Body && !_mobState.IsDead(source))
		{
			_popup.PopupEntity(Loc.GetString("cult-juggernaut-shell-needs-dead"), user, user, PopupType.Medium);
			return;
		}

		if (_pendingConstructs.TryGetValue(shell, out var pending) &&
			_ui.IsUiOpen(shell, RadialSelectorUiKey.Key, pending.User))
		{
			_popup.PopupEntity(Loc.GetString("cult-construct-shell-busy"), shell, user);
			return;
		}

		_pendingConstructs[shell] = new PendingConstructSource(user, source, sourceKind);
		_ui.SetUiState(shell, RadialSelectorUiKey.Key, new TrackedRadialSelectorState(component.Constructs));
		if (!_ui.TryOpenUi(shell, RadialSelectorUiKey.Key, user))
			_pendingConstructs.Remove(shell);
	}

	private void OnConstructSelected(Entity<BloodCultConstructShellComponent> shell, ref RadialSelectorSelectedMessage args)
	{
		var selectedItem = args.SelectedItem;
		if (!_pendingConstructs.Remove(shell, out var pending) || pending.User != args.Actor ||
			!_ui.IsUiOpen(shell.Owner, RadialSelectorUiKey.Key, args.Actor) ||
			!shell.Comp.Constructs.Any(entry => entry.Prototype == selectedItem) ||
			!_prototype.HasIndex<EntityPrototype>(selectedItem) ||
			!Exists(pending.Source) ||
			(pending.SourceKind == BloodCultConstructSourceKind.Body && !_mobState.IsDead(pending.Source)))
		{
			_ui.CloseUi(shell.Owner, RadialSelectorUiKey.Key, args.Actor);
			return;
		}

		if (!TryGetSourceMind(pending.Source, out var mindId, out var mind))
		{
			_ui.CloseUi(shell.Owner, RadialSelectorUiKey.Key, args.Actor);
			return;
		}

		var constructUid = Spawn(selectedItem, _transform.GetMapCoordinates(shell));
		if (!TryComp<BloodCultConstructComponent>(constructUid, out var construct) ||
			!TrySetConstructSource((constructUid, construct), pending.Source, pending.SourceKind,
				"blood_cult_source_container"))
		{
			QueueDel(constructUid);
			_ui.CloseUi(shell.Owner, RadialSelectorUiKey.Key, args.Actor);
			_popup.PopupEntity(Loc.GetString("cult-construct-shell-failed"), shell, args.Actor, PopupType.Medium);
			return;
		}

		_mind.TransferTo(mindId, constructUid, ghostCheckOverride: true, mind: mind);
		_ui.CloseUi(shell.Owner, RadialSelectorUiKey.Key, args.Actor);
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/blink.ogg"), Transform(shell).Coordinates);
		QueueDel(shell);
	}

	private void OnShellShutdown(Entity<BloodCultConstructShellComponent> shell, ref ComponentShutdown args)
	{
		_pendingConstructs.Remove(shell);
	}

	public bool TrySetConstructSource(
		Entity<BloodCultConstructComponent> construct,
		EntityUid source,
		BloodCultConstructSourceKind sourceKind,
		string? containerId = null)
	{
		if (construct.Comp.SourceEntity != null || !Exists(source))
			return false;

		if (containerId != null &&
			(!_container.TryGetContainer(construct, containerId, out var container) ||
			 !_container.Insert(source, container)))
			return false;

		construct.Comp.SourceEntity = source;
		construct.Comp.SourceKind = sourceKind;
		construct.Comp.SourceContainerId = containerId;
		return true;
	}

	private bool TryGetSourceMind(EntityUid source, out EntityUid mindId, out MindComponent mind)
	{
		mindId = default;
		mind = default!;

		if (CompOrNull<MindContainerComponent>(source)?.Mind is not { } sourceMind ||
			!TryComp<MindComponent>(sourceMind, out var sourceMindComponent))
			return false;

		mindId = sourceMind;
		mind = sourceMindComponent;
		return true;
	}

	private readonly record struct PendingConstructSource(
		EntityUid User,
		EntityUid Source,
		BloodCultConstructSourceKind SourceKind);

	private void OnJuggernautDragDropTarget(EntityUid uid, JuggernautComponent component, ref DragDropTargetEvent args)
	{
		// Only allow reactivating inactive juggernauts
		if (!component.IsInactive)
		{
			args.Handled = true;
			return;
		}

		// Verify the dragged entity is a dead body with a mind
		if (!_mobState.IsDead(args.Dragged))
		{
			_popup.PopupEntity(Loc.GetString("cult-juggernaut-shell-needs-dead"), args.User, args.User, PopupType.Medium);
			args.Handled = true;
			return;
		}

		EntityUid? mindId = CompOrNull<MindContainerComponent>(args.Dragged)?.Mind;
		MindComponent? mindComp = CompOrNull<MindComponent>(mindId);
		
		if (mindId == null || mindComp == null)
		{
			_popup.PopupEntity(Loc.GetString("cult-invocation-fail-nosoul"), args.User, args.User, PopupType.Medium);
			args.Handled = true;
			return;
		}

		_ReactivateJuggernautWithBody(args.Dragged, args.User, uid, component);
		args.Handled = true;
	}

	private void _ReactivateJuggernautWithBody(EntityUid body, EntityUid user, EntityUid juggernaut, JuggernautComponent juggComp)
	{
		// Get the mind from the body
		EntityUid? mindId = CompOrNull<MindContainerComponent>(body)?.Mind;
		MindComponent? mindComp = CompOrNull<MindComponent>(mindId);
		
		if (mindId == null || mindComp == null)
		{
			_popup.PopupEntity(Loc.GetString("cult-invocation-fail-nosoul"), user, user, PopupType.Medium);
			return;
		}

		if (!TryComp<BloodCultConstructComponent>(juggernaut, out var construct) ||
			!TrySetConstructSource((juggernaut, construct), body, BloodCultConstructSourceKind.Body,
				"juggernaut_body_container"))
			return;

		// Reactivate the juggernaut.
		juggComp.IsInactive = false;

		// Grant Commune ability to juggernaut if not already granted
		GrantCommuneAction(juggernaut);

		// DON'T heal the juggernaut - it stays in critical state until healed with blood

		// Transfer mind from body to juggernaut
		_mind.TransferTo((EntityUid)mindId, juggernaut, ghostCheckOverride: true, mind: mindComp);
		
		// Ensure juggernaut is in the BloodCultist faction (remove any crew alignment)
		if (TryComp<NpcFactionMemberComponent>(juggernaut, out var npcFaction))
		{
			_npcFaction.ClearFactions((juggernaut, npcFaction), false);
		}
		_npcFaction.AddFaction(juggernaut, BloodCultRuleSystem.BloodCultistFactionId);

		// Play transformation audio
		var coordinates = Transform(juggernaut).Coordinates;
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/blink.ogg"), coordinates);

		// Notify the user
		_popup.PopupEntity(Loc.GetString("cult-juggernaut-reactivated"), user, user, PopupType.Large);
	}

	/// <summary>
	/// Handles alt-fire (right-click) attacks for juggernauts and shades.
	/// When right-clicking without a target, finds the nearest hostile enemy and performs a light attack.
	/// Similar to how zombies bite the nearest enemy on right-click.
	/// </summary>
	private void OnHeavyAttack(HeavyAttackEvent ev, EntitySessionEventArgs args)
	{
		if (args.SenderSession.AttachedEntity is not { } user)
			return;

		// Only handle for juggernauts and shades
		if (!HasComp<JuggernautComponent>(user) && !HasComp<ShadeComponent>(user))
			return;

		// Only handle if there's no specific target (right-click without clicking on an entity)
		// HeavyAttackEvent.Entities contains the entities the client thinks it hit.
		// If there are valid targets (not the user, damageable), let the normal heavy attack handle it.
		// Otherwise, we'll find the nearest enemy below.
		if (ev.Entities != null && ev.Entities.Count > 0)
		{
			foreach (var netEntity in ev.Entities)
			{
				if (TryGetEntity(netEntity, out var entity) && entity != user && HasComp<DamageableComponent>(entity))
				{
					// Valid target exists - let the normal heavy attack system handle this event
					return;
				}
			}
		}

		// No valid target found (or Entities was null/empty) - find the nearest enemy and attack them

		// Get the weapon (should be the entity itself for unarmed attacks)
		if (!_melee.TryGetWeapon(user, out var weaponUid, out var weapon))
			return;

		// Get the melee range
		var range = weapon.Range;

		// Find nearest hostile enemy within range
		EntityUid? nearestEnemy = null;
		float nearestDistance = float.MaxValue;

		var userXform = Transform(user);
		var userPos = _transform.GetWorldPosition(userXform);

		// Get nearby hostiles using faction system
		if (TryComp<NpcFactionMemberComponent>(user, out var factionComp))
		{
			foreach (var hostile in _npcFaction.GetNearbyHostiles((user, factionComp, null), range))
			{
				if (!TryComp<DamageableComponent>(hostile, out _))
					continue;

				var hostilePos = _transform.GetWorldPosition(hostile);
				var distance = (hostilePos - userPos).LengthSquared();

				if (distance < nearestDistance)
				{
					nearestDistance = distance;
					nearestEnemy = hostile;
				}
			}
		}

		// If we found a nearest enemy, perform a light attack on them
		if (nearestEnemy != null && nearestEnemy.Value.IsValid())
		{
			_melee.AttemptLightAttack(user, weaponUid, weapon, nearestEnemy.Value);
		}
	}
}
