namespace Content.Client._Funkystation.VendingMachines;

/// <summary>
/// made so VendingMachineSystem can push state to either the upstream vending ui or the funky keypad ui without knowing which is open
/// </summary>
public interface IVendingMachineBoundUi
{
    void Refresh();
    void UpdateAmounts();
}
