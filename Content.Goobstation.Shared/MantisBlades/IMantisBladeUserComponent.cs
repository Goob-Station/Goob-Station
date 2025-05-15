// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.MantisBlades;

public interface IMantisBladeUserComponent
{
    string BladeProto { get; set; }

    EntityUid? BladeUid { get; set; }

    SoundSpecifier? ExtendSound { get; set; }

    SoundSpecifier? RetractSound { get; set; }
}
