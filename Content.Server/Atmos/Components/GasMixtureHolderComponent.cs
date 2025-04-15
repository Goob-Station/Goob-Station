// SPDX-FileCopyrightText: 2020 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Campbell Suter <znix@znix.xyz>
// SPDX-FileCopyrightText: 2020 ComicIronic <comicironic@gmail.com>
// SPDX-FileCopyrightText: 2020 GlassEclipse <tsymall5@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 a.rudenko <creadth@gmail.com>
// SPDX-FileCopyrightText: 2020 creadth <creadth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 silicons <2003111+silicons@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;

namespace Content.Server.Atmos.Components
{
    [RegisterComponent]
    public sealed partial class GasMixtureHolderComponent : Component, IGasMixtureHolder
    {
        [DataField("air")] public GasMixture Air { get; set; } = new GasMixture();
    }
}