using System.Collections;
using UnityEngine;
using static System.TimeZoneInfo;

public class StarTransition : MonoBehaviour
{
    public Renderer rend;
    public Material mat;
    public float timerDuration = 5f;
    public float transition_time = 0;
    public float minScale;
    public float maxScale;
    public bool isEnabled;

    void Start()
    {
        mat = rend.material;

        // Set numbers
        mat.SetVector("_NoiseScale", new Vector2(0f, 0f));
    }

    void Update()
    {
        if (isEnabled)
        {
            isEnabled = false;
            StartCoroutine(AnimateStarTransition());
        }
    }

    IEnumerator AnimateStarTransition()
    {
        float timer = 0;

        while (timer <= timerDuration)
        {
            timer += Time.deltaTime;
            transition_time = Mathf.Lerp(minScale, maxScale, (timer/timerDuration));
            mat.SetVector("_NoiseScale", new Vector2(transition_time, transition_time));
            yield return null;
        }   

        yield return null;
    }
}
