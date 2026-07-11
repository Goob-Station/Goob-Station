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
using Robust.Client.UserInterface; // Pirate: camera

namespace Content.Client.RoundEnd
{
    public sealed partial class RoundEndSummaryWindow : DefaultWindow // Pirate: camera
    {
        private readonly IFileDialogManager _fileDialogManager; // Pirate: camera
        private readonly IEntityManager _entityManager;
        private readonly TabContainer _roundEndTabs;
        public int RoundId;

        public RoundEndSummaryWindow(string gm, string roundEnd, TimeSpan roundTimeSpan, int roundId,
            RoundEndMessageEvent.RoundEndPlayerInfo[] info, IEntityManager entityManager, IFileDialogManager fileDialogManager) // Pirate: camera
        {
            _entityManager = entityManager;
            _fileDialogManager = fileDialogManager; // Pirate: camera

            MinSize = new Vector2(610, 580); // Pirate: camera

            Title = Loc.GetString("round-end-summary-window-title");

            // The round end window is split into two tabs, one about the round stats
            // and the other is a list of RoundEndPlayerInfo for each player.
            // This tab would be a good place for things like: "x many people died.",
            // "clown slipped the crew x times.", "x shots were fired this round.", etc.
            // Also good for serious info.

            RoundId = roundId;
            _roundEndTabs = new TabContainer(); // Pirate: camera
            _roundEndTabs.AddChild(MakeRoundEndSummaryTab(gm, roundEnd, roundTimeSpan, roundId)); // Pirate: camera
            _roundEndTabs.AddChild(MakePlayerManifestTab(info)); // Pirate: camera
            _roundEndTabs.AddChild(MakeStationReportTab()); //goob
            AddOrUpdatePhotoReportTab(); // Pirate: camera

            ContentsContainer.AddChild(_roundEndTabs);

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
            gamemodeMessage.AddMarkupOrThrow(Loc.GetString("round-end-summary-window-gamemode-name-label",
                ("gamemode", FormattedMessage.EscapeText(gamemode))));
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
                roundEndLabel.SetMarkupPermissive(roundEnd);
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
                    StyleClasses = { StyleClass.PanelDark }, // Pirate: ui fixes
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
                        StyleClasses = { StyleClass.LabelHeading }, // Pirate: ui fixes
                        Margin = new Thickness(0, 0, 6, 0),
                        Text = playerInfo.PlayerICName
                    };
                    playerTitleBox.AddChild(playerNameText);

                    var role = Loc.GetString(playerInfo.Role);
                    var playerRoleText = new Label
                    {
                        VerticalAlignment = VAlignment.Bottom,
                        StyleClasses = { StyleClass.LabelSubText }, // Pirate: ui fixes
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
                        ("lastWords", FormattedMessage.EscapeText(playerInfo.LastWords))));

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
                    var severityKey = totalDamage switch
                    {
                        >= 1000 => "round-end-summary-window-death-severity-catastrophic",
                        >= 750 => "round-end-summary-window-death-severity-devastating",
                        >= 500 => "round-end-summary-window-death-severity-agonizing",
                        >= 300 => "round-end-summary-window-death-severity-painful",
                        >= 200 => "round-end-summary-window-death-severity-brutal",
                        _ => "round-end-summary-window-death-severity-tragic"
                    };

                    var highestDamage = playerInfo.DamagePerGroup
                        .OrderByDescending(kvp => kvp.Value)
                        .First();
                    var typeKey = highestDamage.Key switch
                    {
                        "Burn" => "round-end-summary-window-death-type-fiery",
                        "Brute" => "round-end-summary-window-death-type-crushing",
                        "Toxin" => "round-end-summary-window-death-type-poisonous",
                        "Airloss" => "round-end-summary-window-death-type-suffocating",
                        "Genetic" => "round-end-summary-window-death-type-twisted",
                        "Metaphysical" => "round-end-summary-window-death-type-otherworldly",
                        "Electronic" => "round-end-summary-window-death-type-shocking",
                        _ => "round-end-summary-window-death-type-mysterious",
                    };

                    deathLabel.SetMarkup(
                        Loc.GetString("round-end-summary-window-death",
                            ("severity", Loc.GetString(severityKey)),
                            ("type", Loc.GetString(typeKey))));

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
                            StyleClasses = { StyleClass.PanelLight }, // Pirate: ui fixes
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
            StationReportLabel.SetMarkupPermissive(stationReportText);
            StationReportContainer.AddChild(StationReportLabel);


            StationReportContainerScrollbox.AddChild(StationReportContainer);
            stationReportTab.AddChild(StationReportContainerScrollbox);
            return stationReportTab;
        }
        #endregion
    }

}
