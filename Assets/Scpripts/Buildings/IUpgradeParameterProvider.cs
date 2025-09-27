// ��������� ��� ������� ������: provide upgrade/current params strings ��� UI.
using System.Collections.Generic;

public interface IUpgradeParameterProvider
{
    List<string> GetUpgradeParameters(int currentLevel); // Diffs current -> next.
    List<string> GetCurrentParameters(int currentLevel); // ������ current vals.
}