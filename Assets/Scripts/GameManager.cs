using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int totalCubesPlaced = 0;
    public int cubesPlacedThisRun = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCubePlaced()
    {
        totalCubesPlaced++;
        cubesPlacedThisRun++;
    }

    public void ResetRunStats()
    {
        cubesPlacedThisRun = 0;
    }
}
