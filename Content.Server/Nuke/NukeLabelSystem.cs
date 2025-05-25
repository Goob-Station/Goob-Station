// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Nuke;

/// <summary>
///     This handles labelling an entity with a nuclear bomb label.
/// </summary>
public sealed class NukeLabelSystem : EntitySystem
{
    [Dependency] private readonly NukeSystem _nuke = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<NukeLabelComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, NukeLabelComponent nuke, MapInitEvent args)
    {
        var label = Loc.GetString(nuke.Prefix, ("serial", _nuke.GenerateRandomNumberString(nuke.SerialLength)));
        var meta = MetaData(uid);
        _metaData.SetEntityName(uid, $"{meta.EntityName} ({label})", meta);
    }
}