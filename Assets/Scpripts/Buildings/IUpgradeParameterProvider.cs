// ��������� ��� �������, ��������������� ��������� ��� ���������.
using System.Collections.Generic;

public interface IUpgradeParameterProvider
{
    List<string> GetUpgradeParameters(int currentLevel);
    List<string> GetCurrentParameters(int currentLevel);
}