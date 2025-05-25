// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 ss14-Starlight <ss14-Starlight@outlook.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Starlight.VentCrawling.Components;
using Robust.Shared.Audio;
using Robust.Shared.Containers;

namespace Content.Shared._Starlight.VentCrawling.Components;

[RegisterComponent]
public sealed partial class VentCrawlerHolderComponent : Component
{
    private Container? _container;
    public Container Container
    {
        get => _container ?? throw new InvalidOperationException("Container not initialized");
        set => _container = value;
    }

    [ViewVariables]
    public float StartingTime { get; set; }

    [ViewVariables]
    public float TimeLeft { get; set; }

    public bool IsMoving = false;

    [ViewVariables]
    public EntityUid? PreviousTube { get; set; }

    [ViewVariables]
    public EntityUid? NextTube { get; set; }

    [ViewVariables]
    public Direction PreviousDirection { get; set; } = Direction.Invalid;

    [ViewVariables]
    public EntityUid? CurrentTube { get; set; }

    [ViewVariables]
    public bool FirstEntry { get; set; }

    [ViewVariables]
    public Direction CurrentDirection { get; set; } = Direction.Invalid;

    [ViewVariables]
    public bool IsExitingVentCraws { get; set; }

    public static readonly TimeSpan CrawlDelay = TimeSpan.FromSeconds(0.5);

    public TimeSpan LastCrawl;

    [DataField("crawlSound")]
    public SoundCollectionSpecifier CrawlSound { get; set; } = new ("VentCrawlingSounds", AudioParams.Default.WithVolume(5f));

    [DataField("speed")]
    public float Speed = 0.15f;
}

[ByRefEvent]
public record struct VentCrawlingExitEvent
{
    public TransformComponent? holderTransform;
}