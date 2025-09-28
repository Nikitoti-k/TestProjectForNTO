// ����� �������� ������� �����, ����� �������� �� ������ ����������-���������� ��� ������� + ��������� � ������ � ������� ���������
using UnityEngine;

[CreateAssetMenu(fileName = "GameSceneConfiguration", menuName = "GameSceneConfiguration", order = 1)]
public class GameSceneConfiguration : ScriptableObject
{
    [Header("��������� �����")]
    [Tooltip("������ �������������� �����")]
    [SerializeField] private int gridRadius = 5; 

    [Tooltip("������ ������ �����")]
    [SerializeField] private float cellSize = 2f;

    [Tooltip("��������, ������������ ��� ��������� ����� �����")]
    [SerializeField] private Material gridLineMaterial; 

    [Header("��������� ���������� ������")]
    [Tooltip("����������� ��������������� ������ ��� ����������")]
    [SerializeField] private float buildingScaleFactor = 1f; 

    [Tooltip("������ ������ �����")]
    [SerializeField] private GameObject headquartersPrefab;

    [Tooltip("�������� ��� ����������� ���������� ��������� ������ ��� ������")]
    [SerializeField] private Material previewValidMaterial; 

    [Tooltip("�������� ��� ����������� ������������ ��������� ������ ��� ������")]
    [SerializeField] private Material previewInvalidMaterial; 

    [Header("��������� ���������")]
    [Tooltip("��������� ���������� ������ ������")]
    [SerializeField] private int startingCurrency = 1000; 

    [Tooltip("��������� ���� ��� ������� ������ (���� �� ��������� ���������)")]
    [SerializeField] private float sellPriceMultiplier = 0.8f; 

    [Header("��������� ��������������")]
    [Tooltip("����, ������������ ��� ������")]
    [SerializeField] private LayerMask buildingLayer; 

    [Tooltip("����, ������������ ��� ������")]
    [SerializeField] private LayerMask enemyLayer; 

    [Tooltip("������ �������� ����� ������ ������")]
    [SerializeField] private float spawnRadius = 2f; 

    public int GridRadius => gridRadius;
    public float CellSize => cellSize;
    public Material GridLineMaterial => gridLineMaterial;
    public float BuildingScaleFactor => buildingScaleFactor;
    public GameObject HeadquartersPrefab => headquartersPrefab;
    public Material PreviewValidMaterial => previewValidMaterial;
    public Material PreviewInvalidMaterial => previewInvalidMaterial;
    public int StartingCurrency => startingCurrency;
    public float SellPriceMultiplier => sellPriceMultiplier;
    public LayerMask BuildingLayer => buildingLayer;
    public LayerMask EnemyLayer => enemyLayer;
    public float SpawnRadius => spawnRadius;

    
    public void GenerateGrid(HexGrid grid)
    {
        if (grid != null)
        {
            grid.RegenerateGrid(this);
        }
    }
}