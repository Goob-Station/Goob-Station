namespace Content.Client.Administration.UI.PlayerPanel;

public sealed partial class PlayerPanel
{
    public void SetAccountAge(TimeSpan? age)
    {
        AccountAge.Text = age != null
            ? Loc.GetString("player-panel-account-age",
                ("days", age.Value.Days),
                ("hours", age.Value.Hours % 24),
                ("minutes", age.Value.Minutes % (24 * 60)))
            : null;
    }
}