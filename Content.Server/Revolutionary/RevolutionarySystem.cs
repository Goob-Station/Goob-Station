// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._EinsteinEngines.Language; // Goob Station - Revolutionary Language
using Content.Shared.Revolutionary;
using Content.Shared.Revolutionary.Components; // Goob Station - Revolutionary Language

namespace Content.Server.Revolutionary;

public sealed class RevolutionarySystem : SharedRevolutionarySystem  // Goob Station - Revolutionary Language (entire class body)
{
    [Dependency] private readonly LanguageSystem _languageSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RevolutionaryComponent, ComponentShutdown>(OnRevolutionaryComponentShutdown);
        SubscribeLocalEvent<HeadRevolutionaryComponent, ComponentShutdown>(OnRevolutionaryComponentShutdown);
    }

    public override void OnRevolutionaryComponentStartup<T>(EntityUid someUid, T someComp, ComponentStartup ev)
    {
        base.OnRevolutionaryComponentStartup(someUid, someComp, ev);

        switch (someComp)
        {
            case HeadRevolutionaryComponent headRevComp:
                _languageSystem.AddLanguage(someUid, headRevComp.Language);
                break;
            case RevolutionaryComponent revComp:
                _languageSystem.AddLanguage(someUid, revComp.Language);
                break;
        }
    }

    private void OnRevolutionaryComponentShutdown<T>(EntityUid uid, T component, ComponentShutdown args)
    {
        switch (component)
        {
            case HeadRevolutionaryComponent headRevComp:
                _languageSystem.RemoveLanguage(uid, headRevComp.Language);
                break;
            case RevolutionaryComponent revComp:
                _languageSystem.RemoveLanguage(uid, revComp.Language);
                break;
        }
    }
}
