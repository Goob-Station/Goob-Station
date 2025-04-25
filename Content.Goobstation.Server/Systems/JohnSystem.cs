// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Systems;

public sealed class JohnSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private const string TargetTag = "JohnMind";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MetaDataComponent, ComponentInit>(OnMetadataInit);
        SubscribeLocalEvent<MetaDataComponent, EntityRenamedEvent>(OnRenamed);
    }

    private void OnMetadataInit(EntityUid uid, MetaDataComponent component, ComponentInit args)
    {
        CheckJohn(uid, component.EntityName);
    }

    private void OnRenamed(EntityUid uid, MetaDataComponent component, ref EntityRenamedEvent args)
    {
        CheckJohn(uid, args.NewName);
    }

    private void CheckJohn(EntityUid uid, string? name)
    {
        if (string.IsNullOrEmpty(name) || !name.Contains("John", System.StringComparison.OrdinalIgnoreCase))
            return;

        var tag = _proto.Index<TagPrototype>(TargetTag);
        if (!_tag.HasTag(uid, tag))
            _tag.AddTag(uid, tag);
    }
}

