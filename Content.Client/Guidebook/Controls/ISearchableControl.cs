// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Client.Guidebook.Controls;
public interface ISearchableControl
{
    public bool CheckMatchesSearch(string query);
    /// <summary>
    ///    Sets the hidden state for the control. In simple cases this could just disable/hide it, but you may want more complex behavior for some elements.
    /// </summary>
    public void SetHiddenState(bool state, string query);
}
