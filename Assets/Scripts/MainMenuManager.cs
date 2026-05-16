using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Statistics")]
    public TextMeshProUGUI statsText;

    void Start()
    {
        UpdateStats();
    }

    void UpdateStats()
    {
        if (statsText != null && GameManager.Instance != null)
        {
            int totalCubes = GameManager.Instance.totalCubesPlaced;
            statsText.text = $"Всего кубов поставлено: {totalCubes}";
        }
    }

    public void PlayGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetRunStats();
        }
        SceneManager.LoadScene("SampleScene");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
