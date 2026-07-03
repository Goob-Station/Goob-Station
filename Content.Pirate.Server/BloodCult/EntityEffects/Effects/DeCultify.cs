// SPDX-FileCopyrightText: 2025 Drywink <hugogrethen@gmail.com>
// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Text.Json.Serialization;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Content.Server.Damage.Systems;
using Content.Shared.EntityEffects;
using Content.Shared.BloodCult;
using Content.Shared.Damage.Systems;

namespace Content.Server.EntityEffects.Effects;

//[UsedImplicitly]
public sealed partial class DeCultify : EntityEffect
{
	/// <summary>
	/// Decultification to apply every cycle.
	/// </summary>
	[DataField(required: true)]
	[JsonPropertyName("amount")]
	public float Amount = default!;

	protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
	{
		return "In large quantities, can free a person's mind from servitude to an eldritch entity.";
	}

	public override void Effect(EntityEffectBaseArgs args)
	{
		if (!args.EntityManager.TryGetComponent(args.TargetEntity, out BloodCultistComponent? bloodCultist))
			return;

		var scale = 1.0f;

		if (args is EntityEffectReagentArgs reagentArgs)
		{
			scale = reagentArgs.Scale.Float();
		}

		var oldDeCultification = bloodCultist.DeCultification;
		var newDeCultification = oldDeCultification + (Amount * scale);
		bloodCultist.DeCultification = newDeCultification;

		// If this application causes deconversion (crosses 100 threshold), play sound and knock down
		if (oldDeCultification < 100.0f && newDeCultification >= 100.0f)
		{
			var audioSystem = args.EntityManager.System<SharedAudioSystem>();
			var staminaSystem = args.EntityManager.System<StaminaSystem>();

			// Play holy sound
			audioSystem.PlayPvs(
				new SoundPathSpecifier("/Audio/Effects/holy.ogg"),
				args.TargetEntity,
				AudioParams.Default
			);

			// Apply stamina damage to knock them down
			staminaSystem.TakeStaminaDamage(args.TargetEntity, 100f, visual: false);
		}
	}
}
