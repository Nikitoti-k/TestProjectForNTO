// »нтерфейс дл€ модулей, предоставл€ющих параметры дл€ улучшени€.
using System.Collections.Generic;

public interface IUpgradeParameterProvider
{
    List<string> GetUpgradeParameters(int currentLevel);
    List<string> GetCurrentParameters(int currentLevel);
}