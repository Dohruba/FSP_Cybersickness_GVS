using UnityEngine;

public class RendererUtiliry : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ToggleRenderer(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleRenderer(true);
        }
    }

    public void ToggleRenderer(bool turnOn)
    {
        foreach (Transform t in transform)
        {
            t.GetComponent<MeshRenderer>().enabled = turnOn;
        }
    }
}
