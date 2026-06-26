using UnityEngine;
using UnityEngine.SceneManagement; // ain't it included in UnityEngine already? why do i need to copy it again?

public class lets_play : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }
}
