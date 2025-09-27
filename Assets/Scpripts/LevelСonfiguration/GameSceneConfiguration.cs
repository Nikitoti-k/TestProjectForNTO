// Общие натройки игровой сцены, вынес значение из разных синглтонов-менеджеров для удоства + объединил в группы и добавил пояснялки
using UnityEngine;

[CreateAssetMenu(fileName = "GameSceneConfiguration", menuName = "GameSceneConfiguration", order = 1)]
public class GameSceneConfiguration : ScriptableObject
{
    [Header("Настройки сетки")]
    [Tooltip("Радиус гексагональной сетки")]
    [SerializeField] private int gridRadius = 5; 

    [Tooltip("Размер одного гекса")]
    [SerializeField] private float cellSize = 2f;

    [Tooltip("Материал, используемый для отрисовки линий сетки")]
    [SerializeField] private Material gridLineMaterial; 

    [Header("Настройки размещения зданий")]
    [Tooltip("Коэффициент масштабирования зданий при размещении")]
    [SerializeField] private float buildingScaleFactor = 1f; 

    [Tooltip("Префаб здания штаба")]
    [SerializeField] private GameObject headquartersPrefab;

    [Tooltip("Материал для отображения возможного положения здания при превью")]
    [SerializeField] private Material previewValidMaterial; 

    [Tooltip("Материал для отображения невозможного положения здания при превью")]
    [SerializeField] private Material previewInvalidMaterial; 

    [Header("Настройки экономики")]
    [Tooltip("Начальное количество валюты игрока")]
    [SerializeField] private int startingCurrency = 1000; 

    [Tooltip("Множитель цены при продаже зданий (доля от начальной стоимости)")]
    [SerializeField] private float sellPriceMultiplier = 0.8f; 

    [Header("Настройки взаимодействия")]
    [Tooltip("Слой, используемый для зданий")]
    [SerializeField] private LayerMask buildingLayer; 

    [Tooltip("Слой, используемый для врагов")]
    [SerializeField] private LayerMask enemyLayer; 

    [Tooltip("Радиус разброса точек спавна врагов")]
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