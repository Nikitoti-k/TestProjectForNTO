// SO дл€ врагов: хранит параметры HP, speed, damage, attack rate/range, detection.
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemies/EnemyData", order = 3)]
public class EnemyData : ScriptableObject
{
    public int MaxHealth = 50;
    public float Speed = 3f;
    public int Damage = 10;
    public float AttackRate = 1f;
    public float AttackRange = 1f;
    public float DetectionRange = 5f;
    public int Reward = 10; // Ќовый: валюта за kill.
}