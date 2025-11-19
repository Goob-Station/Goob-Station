// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Shared.Materials;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client.Materials.UI;

public sealed class MaterialStorageUIController : UIController
{
    public void SendLatheEjectMessage(EntityUid uid, string material, int sheetsToEject)
    {
        EntityManager.RaisePredictiveEvent(new EjectMaterialMessage(EntityManager.GetNetEntity(uid), material, sheetsToEject));
    }
}
