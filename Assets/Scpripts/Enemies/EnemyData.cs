using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemies/EnemyData", order = 3)]
public class EnemyData : ScriptableObject
{
    public int MaxHealth = 50;
    public float Speed = 3f;
    public int Damage = 10;
    public float AttackRate = 1f; // Атак в секунду
    public float AttackRange = 1f; // Радиус атаки
}