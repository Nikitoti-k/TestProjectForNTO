// Интерфейс для взаимодействия с постройками: улучшение, продажа, получение данных.
using System.Collections.Generic;
using UnityEngine;

public interface IBuildingInteractable
{
    int GetUpgradeCost();
    int GetSellPrice();
    bool CanUpgrade();
    void Upgrade();
    void Sell();
    Vector3 GetUIPosition();
    List<string> GetUpgradeParameters();
    string GetLevelDisplay();
    string GetBuildingName();
}