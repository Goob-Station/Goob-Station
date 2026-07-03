// SPDX-FileCopyrightText: 2025 Drywink <hugogrethen@gmail.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using Content.Server.Fluids.EntitySystems;
using Content.Server.Body.Systems;
using Content.Server.Body.Components;
using Content.Server.Popups;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Server.Damage.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server.BloodCult.EntitySystems;

/// <summary>
/// Unified system to handle all blood cultist reagent reactions.
/// This includes:
/// - Blood consumption (ingestion) -> causes bleeding, restores blood levels, adds to ritual pool
/// - Sanguine Perniculate (touch) -> heals holy damage
/// </summary>
public sealed class BloodCultistReactionSystem : EntitySystem
{
	[Dependency] private readonly BloodstreamSystem _bloodstream = default!;
	[Dependency] private readonly DamageableSystem _damageable = default!;
	[Dependency] private readonly PopupSystem _popup = default!;
	[Dependency] private readonly SharedAudioSystem _audio = default!;

	public override void Initialize()
	{
		base.Initialize();

		// Single subscription to handle all cultist reagent reactions
		SubscribeLocalEvent<BloodCultistComponent, ReactionEntityEvent>(OnCultistReaction);
	}

	private void OnCultistReaction(EntityUid uid, BloodCultistComponent component, ref ReactionEntityEvent args)
	{
		// Handle blood ingestion
		if (args.Method == ReactionMethod.Ingestion)
		{
			HandleBloodIngestion(uid, ref args);
			return;
		}

		// Handle touch reactions
		if (args.Method == ReactionMethod.Touch)
		{
			// Check for Sanguine Perniculate healing
			// This ONLY heals holy damage. Nothing else.
			if (args.Reagent.ID == "SanguinePerniculate")
			{
				HandleSanguinePerniculateTouch(uid, ref args);
			}
		}
	}

	/// <summary>
	/// Handles blood consumption by cultists.
	/// Used to be used to collect blood for the ritual pool, but that's been removed.
	/// </summary>
	private void HandleBloodIngestion(EntityUid uid, ref ReactionEntityEvent args)
	{
		// Only process sacrifice blood reagents (not Sanguine Perniculate)
		if (!BloodCultConstants.SacrificeBloodReagents.Contains(args.Reagent.ID))
			return;

		var bloodAmount = args.ReagentQuantity.Quantity;
		if (bloodAmount <= 0)
			return;

		// Restore the cultist's blood levels (like Saline)
		// Each unit of consumed blood restores 2 units of their blood volume
		if (TryComp<BloodstreamComponent>(uid, out var bloodstream))
		{
			_bloodstream.TryModifyBloodLevel(uid, bloodAmount.Float() * 2.0f, bloodstream);
			
			/// Commented out below code. Why did I ever think it was a good idea to bleed when you drink blood?
			// Cause brief bleeding (1 unit/second for each 5 units consumed)
			// This represents the blood being processed through their system
			//var bleedAmount = bloodAmount.Float() / 5.0f;
			//if (bleedAmount > 0.5f)
			//{
			//	_bloodstream.TryModifyBleedAmount(uid, bleedAmount, bloodstream);
			//}
			
		}

		/// Commented out, handled by the blooddrinker flag now
		// Heal a very tiny amount of toxin damage (0.5 toxin per 10u blood)
		// This is to make sure blood cultists don't have to worry too much about drinking from the floor.
		//if (TryComp<DamageableComponent>(uid, out var damageable))
		//{
		//	if (damageable.Damage.DamageDict.TryGetValue("Poison", out var toxinDamage) && toxinDamage > 0)
		//	{
		//		var healAmount = bloodAmount.Float() * 0.05f;  // Very tiny: 0.5 per 10u
		//		if (healAmount > 0)
		//		{
		//			var healSpec = new DamageSpecifier();
		//			healSpec.DamageDict.Add("Poison", FixedPoint2.New(-healAmount));
		//			_damageable.TryChangeDamage(uid, healSpec, false, false, damageable);
		//		}
		//	}
		//}

		// Visual feedback
		_popup.PopupEntity(
			Loc.GetString("cult-blood-consumed", ("amount", bloodAmount.Float())),
			uid, uid, PopupType.Small
		);
	}

	/// <summary>
	/// Handles Sanguine Perniculate touch reactions for blood cultists.
	/// When a cultist touches Sanguine Perniculate, it heals their holy damage.
	/// </summary>
	private void HandleSanguinePerniculateTouch(EntityUid uid, ref ReactionEntityEvent args)
	{
		// Check if the cultist has any holy damage
		if (!TryComp<DamageableComponent>(uid, out var damageable))
			return;

		// Get the amount of holy damage
		if (!damageable.Damage.DamageDict.TryGetValue("Holy", out var holyDamage) || holyDamage <= 0)
			return;

		// Calculate healing amount based on Sanguine Perniculate quantity
		// 5u of Sanguine Perniculate heals 1 point of holy damage
		var healAmount = args.ReagentQuantity.Quantity.Float() / 5.0f;
		if (healAmount <= 0)
			return;

		// Don't heal more than the actual holy damage
		healAmount = Math.Min(healAmount, holyDamage.Float());

		// Heal the holy damage
		var healSpec = new DamageSpecifier();
		healSpec.DamageDict.Add("Holy", FixedPoint2.New(-healAmount));
		_damageable.TryChangeDamage(uid, healSpec, false, false, damageable);

		// Visual and audio feedback
		_popup.PopupEntity(
			Loc.GetString("cult-sanguine-perniculate-heal", ("amount", Math.Round(healAmount, 1))),
			uid, uid, PopupType.Medium
		);

		_audio.PlayPvs(
			new SoundPathSpecifier("/Audio/Effects/lightburn.ogg"),
			Transform(uid).Coordinates
		);
	}
}

