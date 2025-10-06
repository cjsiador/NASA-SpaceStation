using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadSceneonStart : MonoBehaviour
{
    [Tooltip("Build index of the scene to load (see File > Build Settings...)")]
    public int sceneIndex = 1;

    [Tooltip("Delay before loading (seconds)")]
    public float delay = 3f;

    [Tooltip("Load mode: Single replaces the current scene, Additive stacks it.")]
    public LoadSceneMode loadMode = LoadSceneMode.Single;

    void Start()
    {
        StartCoroutine(LoadAfterDelay());
    }

    private System.Collections.IEnumerator LoadAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneIndex, loadMode);
    }
}
