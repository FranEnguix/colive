using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulationController : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("Menu");
            // Application.Quit();
    }
}
