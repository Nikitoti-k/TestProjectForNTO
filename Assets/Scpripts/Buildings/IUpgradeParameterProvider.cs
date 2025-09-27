// Интерфейс для модулей зданий: provide upgrade/current params strings для UI.
using System.Collections.Generic;

public interface IUpgradeParameterProvider
{
    List<string> GetUpgradeParameters(int currentLevel); // Diffs current -> next.
    List<string> GetCurrentParameters(int currentLevel); // Только current vals.
}