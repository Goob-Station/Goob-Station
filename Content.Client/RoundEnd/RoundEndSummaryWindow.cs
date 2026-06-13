// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 scuffedjays <yetanotherscuffed@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 KIBORG04 <bossmira4@gmail.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Sailor <109166122+Equivocateur@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <slambamactionman@gmail.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 wafehling <wafehling@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ichaie <167008606+Ichaie@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 JORJ949 <159719201+JORJ949@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 MortalBaguette <169563638+MortalBaguette@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Panela <107573283+AgentePanela@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Poips <Hanakohashbrown@gmail.com>
// SPDX-FileCopyrightText: 2025 PuroSlavKing <103608145+PuroSlavKing@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 Whisper <121047731+QuietlyWhisper@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 blobadoodle <me@bloba.dev>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kamkoi <poiiiple1@gmail.com>
// SPDX-FileCopyrightText: 2025 shibe <95730644+shibechef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 tetra <169831122+Foralemes@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Client.Message;
using Content.Shared.GameTicking;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BoxContainer;
// Goob Station - End of Round Screen
using Content.Client.Stylesheets;
using Content.Shared.Mobs;

namespace Content.Client.RoundEnd
{
    public sealed class RoundEndSummaryWindow : DefaultWindow
    {
        private readonly IEntityManager _entityManager;
        public int RoundId;

        public RoundEndSummaryWindow(string gm, string roundEnd, TimeSpan roundTimeSpan, int roundId,
            RoundEndMessageEvent.RoundEndPlayerInfo[] info, IEntityManager entityManager)
        {
            _entityManager = entityManager;

            MinSize = new Vector2(520, 580);

            Title = Loc.GetString("round-end-summary-window-title");

            // The round end window is split into two tabs, one about the round stats
            // and the other is a list of RoundEndPlayerInfo for each player.
            // This tab would be a good place for things like: "x many people died.",
            // "clown slipped the crew x times.", "x shots were fired this round.", etc.
            // Also good for serious info.

            RoundId = roundId;
            var roundEndTabs = new TabContainer();
            roundEndTabs.AddChild(MakeRoundEndSummaryTab(gm, roundEnd, roundTimeSpan, roundId));
            roundEndTabs.AddChild(MakePlayerManifestTab(info));
            roundEndTabs.AddChild(MakeStationReportTab()); //goob

            ContentsContainer.AddChild(roundEndTabs);

            OpenCenteredRight();
            MoveToFront();
        }

        private BoxContainer MakeRoundEndSummaryTab(string gamemode, string roundEnd, TimeSpan roundDuration, int roundId)
        {
            var roundEndSummaryTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = Loc.GetString("round-end-summary-window-round-end-summary-tab-title")
            };

            var roundEndSummaryContainerScrollbox = new ScrollContainer
            {
                VerticalExpand = true,
                Margin = new Thickness(10),
                HScrollEnabled = false,
            };
            var roundEndSummaryContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical
            };

            //Gamemode Name
            var gamemodeLabel = new RichTextLabel();
            var gamemodeMessage = new FormattedMessage();
            gamemodeMessage.AddMarkupOrThrow(Loc.GetString("round-end-summary-window-round-id-label", ("roundId", roundId)));
            gamemodeMessage.AddText(" ");
            gamemodeMessage.AddMarkupOrThrow(Loc.GetString("round-end-summary-window-gamemode-name-label", ("gamemode", gamemode)));
            gamemodeLabel.SetMessage(gamemodeMessage);
            roundEndSummaryContainer.AddChild(gamemodeLabel);

            //Duration
            var roundTimeLabel = new RichTextLabel();
            roundTimeLabel.SetMarkup(Loc.GetString("round-end-summary-window-duration-label",
                                                   ("hours", roundDuration.Hours),
                                                   ("minutes", roundDuration.Minutes),
                                                   ("seconds", roundDuration.Seconds)));
            roundEndSummaryContainer.AddChild(roundTimeLabel);

            //Round end text
            if (!string.IsNullOrEmpty(roundEnd))
            {
                var roundEndLabel = new RichTextLabel();
                roundEndLabel.SetMarkup(roundEnd);
                roundEndSummaryContainer.AddChild(roundEndLabel);
            }

            roundEndSummaryContainerScrollbox.AddChild(roundEndSummaryContainer);
            roundEndSummaryTab.AddChild(roundEndSummaryContainerScrollbox);

            return roundEndSummaryTab;
        }

        #region Goob Station
        // Everything inside this region is heavily edited for goob.
        private BoxContainer MakePlayerManifestTab(RoundEndMessageEvent.RoundEndPlayerInfo[] playersInfo)
        {
            var playerManifestTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = Loc.GetString("round-end-summary-window-player-manifest-tab-title")
            };

            var playerInfoContainerScrollbox = new ScrollContainer
            {
                VerticalExpand = true,
                Margin = new Thickness(10)
            };
            var playerInfoContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical
            };

            //Put observers at the bottom of the list. Put antags on top.
            var sortedPlayersInfo = playersInfo.OrderBy(p => p.Observer).ThenBy(p => !p.Antag);

            //Create labels for each player info.
            foreach (var playerInfo in sortedPlayersInfo)
            {
                var panel = new PanelContainer
                {
                    StyleClasses = { StyleNano.StyleClassBackgroundBaseDark },
                    Margin = new Thickness(0, 0, 0, 6)
                };

                var hBox = new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal,
                    VerticalExpand = true
                };

                if (playerInfo.PlayerNetEntity != null)
                {
                    hBox.AddChild(new SpriteView(playerInfo.PlayerNetEntity.Value, _entityManager)
                    {
                        OverrideDirection = Direction.South,
                        VerticalAlignment = VAlignment.Center,
                        SetSize = new Vector2(64, 64),
                        VerticalExpand = true,
                        Stretch = SpriteView.StretchMode.Fill,
                        Margin = new Thickness(3, 0, 3, 0)
                    });
                }

                var textVBox = new BoxContainer
                {
                    Orientation = LayoutOrientation.Vertical,
                    VerticalExpand = true,
                    SeparationOverride = 2,
                };

                var playerTitleBox = new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal,
                };

                var playerInfoText = new RichTextLabel
                {
                    VerticalAlignment = VAlignment.Center,
                    VerticalExpand = true,
                };

                if (playerInfo.PlayerICName != null)
                {
                    var playerNameText = new Label
                    {
                        VerticalAlignment = VAlignment.Bottom,
                        StyleClasses = { StyleNano.StyleClassLabelHeading },
                        Margin = new Thickness(0, 0, 6, 0),
                        Text = playerInfo.PlayerICName
                    };
                    playerTitleBox.AddChild(playerNameText);

                    var role = Loc.GetString(playerInfo.Role);
                    var playerRoleText = new Label
                    {
                        VerticalAlignment = VAlignment.Bottom,
                        StyleClasses = { StyleNano.StyleClassLabelSubText },
                        Text = Loc.GetString("round-end-summary-window-player-name",
                            ("player", playerInfo.PlayerOOCName))
                    };

                    if (role != "Unknown")
                        playerRoleText.Text = Loc.GetString("round-end-summary-window-player-name-role",
                                ("role", role),
                                ("player", playerInfo.PlayerOOCName));

                    playerTitleBox.AddChild(playerRoleText);
                }

                textVBox.AddChild(playerTitleBox);

                if (!string.IsNullOrWhiteSpace(playerInfo.LastWords))
                {
                    var playerLastWordsText = new RichTextLabel
                    {
                        VerticalAlignment = VAlignment.Center,
                        VerticalExpand = true,
                    };

                    playerLastWordsText.SetMarkup(Loc.GetString("round-end-summary-window-last-words",
                        ("lastWords", playerInfo.LastWords)));

                    textVBox.AddChild(playerLastWordsText);
                }

                var hDeathBox = new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal,
                };

                var deathLabel = new RichTextLabel
                {
                    VerticalAlignment = VAlignment.Center,
                    VerticalExpand = true,
                };

                textVBox.AddChild(deathLabel);

                if (playerInfo.EntMobState == MobState.Dead
                    && playerInfo.DamagePerGroup.Values.Any(v => v > 0))
                {
                    var totalDamage = playerInfo.DamagePerGroup.Values.Sum(static v => (decimal) v);
                    var severityAdj = totalDamage switch
                    {
                        >= 1000 => "catastrophic",
                        >= 750 => "devastating",
                        >= 500 => "agonizing",
                        >= 300 => "painful",
                        >= 200 => "brutal",
                        _ => "tragic"
                    };

                    var highestDamage = playerInfo.DamagePerGroup
                        .OrderByDescending(kvp => kvp.Value)
                        .First();
                    var typeAdj = highestDamage.Key switch
                    {
                        "Burn" => "fiery",
                        "Brute" => "crushing",
                        "Toxin" => "poisonous",
                        "Airloss" => "suffocating",
                        "Genetic" => "twisted",
                        "Metaphysical" => "otherworldly",
                        "Electronic" => "shocking",
                        _ => "mysterious",
                    };

                    deathLabel.SetMarkup(
                        Loc.GetString("round-end-summary-window-death",
                            ("severity", severityAdj),
                            ("type", typeAdj)));

                    var damageTable = new GridContainer
                    {
                        Columns = playerInfo.DamagePerGroup.Count,
                    };

                    foreach (var damage in playerInfo.DamagePerGroup)
                    {
                        if (damage.Value <= 0)
                            continue;

                        var color = damage.Key switch
                        {
                            "Burn" => Color.Orange,
                            "Brute" => Color.Red,
                            "Toxin" => Color.Green,
                            "Airloss" => Color.Blue,
                            "Genetic" => Color.Cyan,
                            "Metaphysical" => Color.Purple,
                            "Electronic" => Color.DarkOrange,
                            _ => Color.White,
                        };
                        var damagePanel = new PanelContainer
                        {
                            StyleClasses = { StyleNano.StyleClassBackgroundBaseLight },
                            Margin = new Thickness(2, 2, 2, 2)
                        };
                        var damageBox = new BoxContainer
                        {
                            Orientation = LayoutOrientation.Vertical,
                            Margin = new Thickness(1)
                        };
                        var valueLabel = new Label
                        {
                            Text = Math.Round((float) damage.Value).ToString(),
                            FontColorOverride = color,
                            HorizontalAlignment = HAlignment.Center,
                            VerticalAlignment = VAlignment.Center,
                        };
                        var headerLabel = new Label
                        {
                            Text = damage.Key,
                            FontColorOverride = Color.Gray,
                            HorizontalAlignment = HAlignment.Center,
                            VerticalAlignment = VAlignment.Center,
                        };
                        damagePanel.AddChild(damageBox);
                        damageBox.AddChild(valueLabel);
                        damageBox.AddChild(headerLabel);
                        damageTable.AddChild(damagePanel);
                    }

                    textVBox.AddChild(damageTable);
                }
                else if (playerInfo.EntMobState == MobState.Invalid)
                {
                    deathLabel.SetMarkup(Loc.GetString("round-end-summary-window-death-unknown"));
                }

                hBox.AddChild(textVBox);
                panel.AddChild(hBox);
                playerInfoContainer.AddChild(panel);
            }

            playerInfoContainerScrollbox.AddChild(playerInfoContainer);
            playerManifestTab.AddChild(playerInfoContainerScrollbox);

            return playerManifestTab;
        }
        private BoxContainer MakeStationReportTab()
        {
            //gets the stationreport varibible and sets the station report tab text to it if the map doesn't have a tablet will say No station report submitted
            var stationReportSystem = _entityManager.System<Content.Goobstation.Common.StationReport.StationReportSystem>();
            string stationReportText = stationReportSystem.StationReportText ?? Loc.GetString("no-station-report-summited");
            var stationReportTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = Loc.GetString("round-end-summary-window-station-report-tab-title")
            };
            var StationReportContainerScrollbox = new ScrollContainer
            {
                VerticalExpand = true,
                Margin = new Thickness(10),
                HScrollEnabled = false,
            };
            var StationReportContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical
            };
            var StationReportLabel = new RichTextLabel();
            var StationReportmessage = new FormattedMessage();
            StationReportmessage.AddMarkupOrThrow(stationReportText);
            StationReportLabel.SetMessage(StationReportmessage);
            StationReportContainer.AddChild(StationReportLabel);


            StationReportContainerScrollbox.AddChild(StationReportContainer);
            stationReportTab.AddChild(StationReportContainerScrollbox);
            return stationReportTab;
        }
        #endregion
    }

}
