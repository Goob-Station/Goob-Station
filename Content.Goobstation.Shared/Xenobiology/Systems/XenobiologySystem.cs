// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Examine;
using Content.Shared.Jittering;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

/// <summary>
///     This handles the server-side of Xenobiology.
///     Why is it in shared again if it handles the server-side part?
/// </summary>
public sealed partial class XenobiologySystem : EntitySystem
{
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private TimeSpan _updateInterval;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeTaming();
        SubscribeBreeding();

        SubscribeLocalEvent<SlimeComponent, ExaminedEvent>(OnExamined);
        Subs.CVar(_cfg, GoobCVars.BreedingInterval, x => _updateInterval = TimeSpan.FromSeconds(x), true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateMitosis();
    }

    private void OnExamined(Entity<SlimeComponent> slime, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange || _net.IsClient)
            return;

        if (slime.Comp.Tamer == args.Examiner)
            args.PushMarkup(Loc.GetString("slime-examined-tamer"));

        if (slime.Comp.Stomach.Count > 0)
            args.PushMarkup(Loc.GetString("slime-examined-stomach"));
    }

    /// <summary>
    /// Returns the extract associated by the slimes breed.
    /// </summary>
    /// <param name="slime">The slime entity.</param>
    /// <returns>Grey if no breed can be found.</returns>
    public EntProtoId GetProducedExtract(Entity<SlimeComponent> slime)
        => _proto.Resolve(slime.Comp.Breed, out var breedPrototype)
            ? breedPrototype.ProducedExtract
            : slime.Comp.DefaultExtract;
}
