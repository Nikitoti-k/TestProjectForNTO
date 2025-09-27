using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    private string savePath;

    private void Awake()
    {
        savePath = System.IO.Path.Combine(Application.persistentDataPath, "save.json");

        if (newGameButton == null || continueButton == null)
        {
            Debug.LogError("MainUIManager: Missing button references!");
            return;
        }

        newGameButton.onClick.AddListener(StartNewGame);
        continueButton.onClick.AddListener(ContinueGame);

        // ��������� ������� ���������� � ����������/������������ ������ "����������".
        continueButton.interactable = System.IO.File.Exists(savePath);
    }

    private void StartNewGame()
    {
        // ������� ����������, ���� ��� ����������.
        if (System.IO.File.Exists(savePath))
        {
            System.IO.File.Delete(savePath);
            Debug.Log("MainUIManager: Save deleted for new game.");
        }
        SceneManager.LoadScene("GameScene");
    }

    private void ContinueGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}