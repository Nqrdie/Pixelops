using Newtonsoft.Json.Bson;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
