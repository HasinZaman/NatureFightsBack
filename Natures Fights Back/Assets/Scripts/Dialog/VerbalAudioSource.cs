using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class AudioSource : MonoBehaviour
{
    [SerializeField]
    private GameObject dialogCanvas;
    [SerializeField]
    private List<(TextMeshProUGUI mesh, float size, float timestamp)> text = new List<(TextMeshProUGUI, float, float)>();

    private float lifetime = 0;

    [SerializeField]
    private AnimationCurve fontSizeCurve = AnimationCurve.EaseInOut(0, 1, 0, 1);
    [SerializeField]
    private AnimationCurve textLocationCurve = AnimationCurve.EaseInOut(0, 1, 0, 1);

    [SerializeField]
    private float completionStep = 1;

    void Start()
    {
        this.dialogCanvas = GameObject.FindGameObjectsWithTag("Dialog_Canvas")[0];

        addText("Hello world");
    }


    void Update()
    {
        Vector3 camPos = Camera.main.transform.position;

        float deltaTime = Time.deltaTime;

        for(int i1 = 0; i1 < this.text.Count; i1++)
        {
            (TextMeshProUGUI mesh, float sizeGoal, float timestamp) = this.text[i1];

            //update position
            mesh.transform.position = updateTextPosition
            (
                mesh.transform.position,
                textPositionGoal(this.text.Count - 1 - i1, new Vector3(0, 25), new Vector3(0, 10)),
                Time.deltaTime,
                0.5f
            );
            //update size
            mesh.fontSize = updateFontSize(mesh.fontSize, sizeGoal, 2f, Time.deltaTime);

            //this.text[i1] = (mesh, sizeGoal, duration);
        }
        //iterate through text
        //update position

        if (this.text.Count > 0)
        {
            lifetime += Time.deltaTime;
        }
    }

    private float updateFontSize(float current, float goal, float springConstant, float deltaTime)
    {
        float delta = goal - current;

        float newSize = current + delta * springConstant * deltaTime; //replace with spring

        if (Mathf.Abs(newSize - goal) <= 0.01f)
        {
            newSize = goal;
        }

        return newSize;
    }
    private Vector3 updateTextPosition(Vector3 current, Vector3 goal, float timestamp, float speed)
    {
        const float speedFactor = 1;//linear speed

        return Vector3.Lerp(current, goal, speed * (this.lifetime - timestamp));
        
        //return Camera.main.WorldToScreenPoint(this.transform.position);
    }
    private Vector3 textPositionGoal(int index, Vector3 offset, Vector3 travelDirection)
    {
        Vector3 startPos = offset + Camera.main.WorldToScreenPoint(this.transform.position);
        return startPos + travelDirection * index;;
    }
    public void addText(string text)
    {
        GameObject tmp = AudioSource.createTextMesh();
        tmp.transform.parent = this.dialogCanvas.transform;
        tmp.transform.position = Camera.main.WorldToScreenPoint(this.transform.position);
        TextMeshProUGUI textMesh = tmp.GetComponent<TextMeshProUGUI>();

        textMesh.text = text;//could be replaced with an iterator that updates the text progamatically

        textMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
        textMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
        //textMesh.enableAutoSizing = true;
        textMesh.fontSize = 0;

        this.text.Add((textMesh, 10, this.lifetime));
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
