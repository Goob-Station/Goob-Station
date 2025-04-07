// SPDX-FileCopyrightText: 2023 08A <git@08a.re>
// SPDX-FileCopyrightText: 2018 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 Hugal31 <hugo.laloge@gmail.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2024 JoeHammad1844 <130668733+JoeHammad1844@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Michael Phillips <1194692+MeltedPixel@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Miro Kavaliou <miraslauk@gmail.com>
// SPDX-FileCopyrightText: 2024 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2019 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2019 Silver <Silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2021 Swept <sweptwastaken@protonmail.com>
// SPDX-FileCopyrightText: 2023 Vasilis The Pikachu <vascreeper@yahoo.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 derek <xderek.luix@gmail.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2020 moneyl <8206401+Moneyl@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 wafehling <wafehling@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 yglop <95057024+yglop@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Input;

namespace Content.Shared.Input
{
    [KeyFunctions]
    public static class ContentKeyFunctions
    {
        public static readonly BoundKeyFunction UseItemInHand = "ActivateItemInHand";
        public static readonly BoundKeyFunction AltUseItemInHand = "AltActivateItemInHand";
        public static readonly BoundKeyFunction ActivateItemInWorld = "ActivateItemInWorld";
        public static readonly BoundKeyFunction AltActivateItemInWorld = "AltActivateItemInWorld";
        public static readonly BoundKeyFunction Drop = "Drop";
        public static readonly BoundKeyFunction ExamineEntity = "ExamineEntity";
        public static readonly BoundKeyFunction FocusChat = "FocusChatInputWindow";
        public static readonly BoundKeyFunction FocusLocalChat = "FocusLocalChatWindow";
        public static readonly BoundKeyFunction FocusEmote = "FocusEmote";
        public static readonly BoundKeyFunction FocusWhisperChat = "FocusWhisperChatWindow";
        public static readonly BoundKeyFunction FocusRadio = "FocusRadioWindow";
        public static readonly BoundKeyFunction FocusLOOC = "FocusLOOCWindow";
        public static readonly BoundKeyFunction FocusOOC = "FocusOOCWindow";
        public static readonly BoundKeyFunction FocusAdminChat = "FocusAdminChatWindow";
        public static readonly BoundKeyFunction FocusDeadChat = "FocusDeadChatWindow";
        public static readonly BoundKeyFunction FocusCollectiveMindChat = "FocusCollectiveMindChatWindow"; // Goobstation - Starlight collective mind port
        public static readonly BoundKeyFunction FocusConsoleChat = "FocusConsoleChatWindow";
        public static readonly BoundKeyFunction CycleChatChannelForward = "CycleChatChannelForward";
        public static readonly BoundKeyFunction CycleChatChannelBackward = "CycleChatChannelBackward";
        public static readonly BoundKeyFunction EscapeContext = "EscapeContext";
        public static readonly BoundKeyFunction OpenCharacterMenu = "OpenCharacterMenu";
        public static readonly BoundKeyFunction OpenEmotesMenu = "OpenEmotesMenu";
        public static readonly BoundKeyFunction OpenCraftingMenu = "OpenCraftingMenu";
        public static readonly BoundKeyFunction OpenGuidebook = "OpenGuidebook";
        public static readonly BoundKeyFunction OpenInventoryMenu = "OpenInventoryMenu";
        public static readonly BoundKeyFunction SmartEquipBackpack = "SmartEquipBackpack";
        public static readonly BoundKeyFunction SmartEquipBelt = "SmartEquipBelt";
        public static readonly BoundKeyFunction SmartEquipBack = "SmartEquipBack"; // Goobstation - Smart equip to back
        public static readonly BoundKeyFunction OpenBackpack = "OpenBackpack";
        public static readonly BoundKeyFunction OpenBelt = "OpenBelt";
        public static readonly BoundKeyFunction OpenAHelp = "OpenAHelp";
        public static readonly BoundKeyFunction SwapHands = "SwapHands";
        public static readonly BoundKeyFunction MoveStoredItem = "MoveStoredItem";
        public static readonly BoundKeyFunction RotateStoredItem = "RotateStoredItem";
        public static readonly BoundKeyFunction SaveItemLocation = "SaveItemLocation";
        public static readonly BoundKeyFunction ThrowItemInHand = "ThrowItemInHand";
        public static readonly BoundKeyFunction TryPullObject = "TryPullObject";
        public static readonly BoundKeyFunction MovePulledObject = "MovePulledObject";
        public static readonly BoundKeyFunction ReleasePulledObject = "ReleasePulledObject";
        public static readonly BoundKeyFunction MouseMiddle = "MouseMiddle";
        public static readonly BoundKeyFunction RotateObjectClockwise = "RotateObjectClockwise";
        public static readonly BoundKeyFunction RotateObjectCounterclockwise = "RotateObjectCounterclockwise";
        public static readonly BoundKeyFunction FlipObject = "FlipObject";
        public static readonly BoundKeyFunction ToggleRoundEndSummaryWindow = "ToggleRoundEndSummaryWindow";
        public static readonly BoundKeyFunction OpenEntitySpawnWindow = "OpenEntitySpawnWindow";
        public static readonly BoundKeyFunction OpenSandboxWindow = "OpenSandboxWindow";
        public static readonly BoundKeyFunction OpenTileSpawnWindow = "OpenTileSpawnWindow";
        public static readonly BoundKeyFunction OpenDecalSpawnWindow = "OpenDecalSpawnWindow";
        public static readonly BoundKeyFunction OpenAdminMenu = "OpenAdminMenu";
        public static readonly BoundKeyFunction TakeScreenshot = "TakeScreenshot";
        public static readonly BoundKeyFunction TakeScreenshotNoUI = "TakeScreenshotNoUI";
        public static readonly BoundKeyFunction ToggleFullscreen = "ToggleFullscreen";
        public static readonly BoundKeyFunction Point = "Point";
        public static readonly BoundKeyFunction ZoomOut = "ZoomOut";
        public static readonly BoundKeyFunction ZoomIn = "ZoomIn";
        public static readonly BoundKeyFunction ResetZoom = "ResetZoom";
        public static readonly BoundKeyFunction ToggleStanding = "ToggleStanding"; // WD EDIT

        // Shitmed Change Start
        public static readonly BoundKeyFunction TargetHead = "TargetHead";
        public static readonly BoundKeyFunction TargetTorso = "TargetTorso";
        public static readonly BoundKeyFunction TargetLeftArm = "TargetLeftArm";
        public static readonly BoundKeyFunction TargetLeftHand = "TargetLeftHand";
        public static readonly BoundKeyFunction TargetRightArm = "TargetRightArm";
        public static readonly BoundKeyFunction TargetRightHand = "TargetRightHand";
        public static readonly BoundKeyFunction TargetLeftLeg = "TargetLeftLeg";
        public static readonly BoundKeyFunction TargetLeftFoot = "TargetLeftFoot";
        public static readonly BoundKeyFunction TargetRightLeg = "TargetRightLeg";
        public static readonly BoundKeyFunction TargetRightFoot = "TargetRightFoot";
        // Shitmed Change End

        public static readonly BoundKeyFunction ArcadeUp = "ArcadeUp";
        public static readonly BoundKeyFunction ArcadeDown = "ArcadeDown";
        public static readonly BoundKeyFunction ArcadeLeft = "ArcadeLeft";
        public static readonly BoundKeyFunction ArcadeRight = "ArcadeRight";
        public static readonly BoundKeyFunction Arcade1 = "Arcade1";
        public static readonly BoundKeyFunction Arcade2 = "Arcade2";
        public static readonly BoundKeyFunction Arcade3 = "Arcade3";

        public static readonly BoundKeyFunction OpenActionsMenu = "OpenAbilitiesMenu";
        public static readonly BoundKeyFunction ShuttleStrafeLeft = "ShuttleStrafeLeft";
        public static readonly BoundKeyFunction ShuttleStrafeUp = "ShuttleStrafeUp";
        public static readonly BoundKeyFunction ShuttleStrafeRight = "ShuttleStrafeRight";
        public static readonly BoundKeyFunction ShuttleStrafeDown = "ShuttleStrafeDown";
        public static readonly BoundKeyFunction ShuttleRotateLeft = "ShuttleRotateLeft";
        public static readonly BoundKeyFunction ShuttleRotateRight = "ShuttleRotateRight";
        public static readonly BoundKeyFunction ShuttleBrake = "ShuttleBrake";

        public static readonly BoundKeyFunction Hotbar0 = "Hotbar0";
        public static readonly BoundKeyFunction Hotbar1 = "Hotbar1";
        public static readonly BoundKeyFunction Hotbar2 = "Hotbar2";
        public static readonly BoundKeyFunction Hotbar3 = "Hotbar3";
        public static readonly BoundKeyFunction Hotbar4 = "Hotbar4";
        public static readonly BoundKeyFunction Hotbar5 = "Hotbar5";
        public static readonly BoundKeyFunction Hotbar6 = "Hotbar6";
        public static readonly BoundKeyFunction Hotbar7 = "Hotbar7";
        public static readonly BoundKeyFunction Hotbar8 = "Hotbar8";
        public static readonly BoundKeyFunction Hotbar9 = "Hotbar9";
        // Goobstation - Extra hotbar hotkeys
        public static readonly BoundKeyFunction HotbarShift0 = "HotbarShift0";
        public static readonly BoundKeyFunction HotbarShift1 = "HotbarShift1";
        public static readonly BoundKeyFunction HotbarShift2 = "HotbarShift2";
        public static readonly BoundKeyFunction HotbarShift3 = "HotbarShift3";
        public static readonly BoundKeyFunction HotbarShift4 = "HotbarShift4";
        public static readonly BoundKeyFunction HotbarShift5 = "HotbarShift5";
        public static readonly BoundKeyFunction HotbarShift6 = "HotbarShift6";
        public static readonly BoundKeyFunction HotbarShift7 = "HotbarShift7";
        public static readonly BoundKeyFunction HotbarShift8 = "HotbarShift8";
        public static readonly BoundKeyFunction HotbarShift9 = "HotbarShift9";

        public static BoundKeyFunction[] GetHotbarBoundKeys() =>
            new[]
            {
                Hotbar1, Hotbar2, Hotbar3, Hotbar4, Hotbar5, Hotbar6, Hotbar7, Hotbar8, Hotbar9, Hotbar0,
                HotbarShift1, HotbarShift2, HotbarShift3, HotbarShift4, HotbarShift5, HotbarShift6, HotbarShift7, HotbarShift8, HotbarShift9, HotbarShift0
            };

        public static readonly BoundKeyFunction Vote0 = "Vote0";
        public static readonly BoundKeyFunction Vote1 = "Vote1";
        public static readonly BoundKeyFunction Vote2 = "Vote2";
        public static readonly BoundKeyFunction Vote3 = "Vote3";
        public static readonly BoundKeyFunction Vote4 = "Vote4";
        public static readonly BoundKeyFunction Vote5 = "Vote5";
        public static readonly BoundKeyFunction Vote6 = "Vote6";
        public static readonly BoundKeyFunction Vote7 = "Vote7";
        public static readonly BoundKeyFunction Vote8 = "Vote8";
        public static readonly BoundKeyFunction Vote9 = "Vote9";
        public static readonly BoundKeyFunction EditorCopyObject = "EditorCopyObject";
        public static readonly BoundKeyFunction EditorFlipObject = "EditorFlipObject";
        public static readonly BoundKeyFunction InspectEntity = "InspectEntity";

        public static readonly BoundKeyFunction MappingUnselect = "MappingUnselect";
        public static readonly BoundKeyFunction SaveMap = "SaveMap";
        public static readonly BoundKeyFunction MappingEnablePick = "MappingEnablePick";
        public static readonly BoundKeyFunction MappingEnableDelete = "MappingEnableDelete";
        public static readonly BoundKeyFunction MappingPick = "MappingPick";
        public static readonly BoundKeyFunction MappingRemoveDecal = "MappingRemoveDecal";
        public static readonly BoundKeyFunction MappingCancelEraseDecal = "MappingCancelEraseDecal";
        public static readonly BoundKeyFunction MappingOpenContextMenu = "MappingOpenContextMenu";
    }
}
