// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Forensics;
using Content.Server.Humanoid;
using Content.Server.IdentityManagement;
using Content.Shared._Shitmed.StatusEffects;
using Content.Shared.DetailExaminable;
using Content.Shared.Forensics;
using Content.Shared.Humanoid;
using Content.Shared.Preferences;
using Content.Shared.Popups;
using Content.Shared.Forensics.Components;

namespace Content.Server._Shitmed.StatusEffects;

public sealed class ScrambleDnaEffectSystem : EntitySystem
{
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly ForensicsSystem _forensicsSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<ScrambleDnaEffectComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, ScrambleDnaEffectComponent component, ComponentInit args) =>
        Scramble(uid);

    public void Scramble(EntityUid uid)
    {
        if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
            return;

        var newProfile = HumanoidCharacterProfile.RandomWithSpecies(humanoid.Species);
        _humanoidAppearance.LoadProfile(uid, newProfile, humanoid);
        _metaData.SetEntityName(uid, newProfile.Name);

        if (!TryComp<DnaComponent>(uid, out var dna))
            return;

        dna.DNA = _forensicsSystem.GenerateDNA();

        var ev = new GenerateDnaEvent { Owner = uid, DNA = dna.DNA };
        RaiseLocalEvent(uid, ref ev);

        if (!TryComp<FingerprintComponent>(uid, out var fingerprint))
            return;

        fingerprint.Fingerprint = _forensicsSystem.GenerateFingerprint();
        RemComp<DetailExaminableComponent>(uid);
        _identity.QueueIdentityUpdate(uid);

        _popup.PopupEntity(Loc.GetString("scramble-implant-activated-popup"), uid, uid);
    }


}
