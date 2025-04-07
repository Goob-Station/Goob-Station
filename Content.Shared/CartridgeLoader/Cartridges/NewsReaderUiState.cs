// SPDX-FileCopyrightText: 2023 MishaUnity <81403616+MishaUnity@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;
using Content.Shared.MassMedia.Systems;

namespace Content.Shared.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class NewsReaderBoundUserInterfaceState : BoundUserInterfaceState
{
    public NewsArticle Article;
    public int TargetNum;
    public int TotalNum;
    public bool NotificationOn;

    public NewsReaderBoundUserInterfaceState(NewsArticle article, int targetNum, int totalNum, bool notificationOn)
    {
        Article = article;
        TargetNum = targetNum;
        TotalNum = totalNum;
        NotificationOn = notificationOn;
    }
}

[Serializable, NetSerializable]
public sealed class NewsReaderEmptyBoundUserInterfaceState : BoundUserInterfaceState
{
    public bool NotificationOn;

    public NewsReaderEmptyBoundUserInterfaceState(bool notificationOn)
    {
        NotificationOn = notificationOn;
    }
}