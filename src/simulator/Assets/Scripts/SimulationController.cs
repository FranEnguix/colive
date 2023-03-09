using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulationController : MonoBehaviour
{   private Camera activeCamera;
    private Camera mainCamera;
    private Camera cenitalCamera;

    void Start ()
	{
		mainCamera =GameObject.FindWithTag ("MainCamera").GetComponent<Camera>();
		mainCamera.GetComponent<AudioListener>().enabled = true;
		cenitalCamera = GameObject.Find("Cenital Camera").GetComponent<Camera>();
		cenitalCamera.GetComponent<AudioListener>().enabled = false;
		cenitalCamera.enabled = false;
		activeCamera = mainCamera;

        Debug.Log ("Inicializacion control");
	}
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("Menu");
            // Application.Quit();

        if (Input.GetKeyDown(KeyCode.M)) {
			activeCamera.enabled=false;
			activeCamera.GetComponent<AudioListener>().enabled = false;
			activeCamera = mainCamera;
			activeCamera.enabled=true;
			activeCamera.GetComponent<AudioListener>().enabled = true;

			Debug.Log ("Post cambio");
		}

		if (Input.GetKeyDown(KeyCode.T)) {
			activeCamera.enabled=false;
			activeCamera.GetComponent<AudioListener>().enabled = false;
			activeCamera = cenitalCamera;
			activeCamera.enabled=true;
			activeCamera.GetComponent<AudioListener>().enabled = true;
			Debug.Log ("T cambio");
		}
        
    }
}
