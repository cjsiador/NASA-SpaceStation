using UnityEngine;

public class StayStationary : MonoBehaviour
{

    [SerializeField]
    private Transform startLoc;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startLoc.position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = startLoc.position;
    }
}
