using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class AudioSource : MonoBehaviour
{
    public GameObject dialogCanvas;
    public List<(TextMeshProUGUI mesh, float size, float timestamp)> text = new List<(TextMeshProUGUI, float, float)>();
    // Start is called before the first frame update
    void Start()
    {
        this.dialogCanvas = GameObject.FindGameObjectsWithTag("Dialog_Canvas")[0];

        addText("Hello world");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 camPos = Camera.main.transform.position;

        foreach ((TextMeshProUGUI mesh, float size, float timestamp) in this.text)
        {
            mesh.transform.position = UIPos(this.transform.position);
        }
        //iterate through text
        //update position
    }

    private Vector3 UIPos(Vector3 pos)
    {
        Transform camera = Camera.main.transform;

        Vector3 delta = Camera.main.transform.position - pos;

        Vector3 x = (camera.localToWorldMatrix * Vector3.right);
        x += -1 * camera.position;
        Vector3 y = (camera.localToWorldMatrix * Vector3.up);
        y += -1* camera.position;

        return new Vector3(Vector3.Dot(delta, x.normalized)/x.magnitude, Vector3.Dot(delta, y.normalized) / y.magnitude, 0);
    }

    public void addText(string text)
    {
        GameObject tmp = AudioSource.createTextMesh();
        tmp.transform.parent = this.dialogCanvas.transform;
        TextMeshProUGUI textMesh = tmp.GetComponent<TextMeshProUGUI>();

        textMesh.text = text;//could be replaced with an iterator that updates the text progamatically

        textMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
        textMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
        textMesh.fontSize = 0;

        this.text.Add((textMesh, 1, Time.time));
        //create text object
        //start with size zero

    }

    private static GameObject createTextMesh()
    {
        // progmatically initalize gameobject
        return new GameObject
        (
            "",
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
    }
}
