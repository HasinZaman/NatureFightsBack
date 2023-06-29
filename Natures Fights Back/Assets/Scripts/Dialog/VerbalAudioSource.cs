using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class VerbalAudioSource : MonoBehaviour
{
    [SerializeField]
    private GameObject dialogCanvas;
    [SerializeField]
    private List<(TextMeshProUGUI mesh, float size, float timestamp)> text = new List<(TextMeshProUGUI, float, float)>();

    private float lifetime = 0;
    private float lastUpdateTimestamp = 0;

    [SerializeField]
    private AnimationCurve fontSizeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField]
    private AnimationCurve textLocationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField]
    private const float maxLifetime = 5f;
    [SerializeField]
    private (uint soft, uint hard) maxText = (1, 3);

    void Start()
    {
        this.dialogCanvas = GameObject.FindGameObjectsWithTag("Dialog_Canvas")[0];
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
                0.5f
            );
            //update size
            mesh.fontSize = updateFontSize(mesh.fontSize, sizeGoal, 1f);
        }

        //check if last element has existed past life time or hard limit
        if (0 < this.text.Count)
        {
            int last = 0;

            bool softLimit = this.maxText.soft <= this.text.Count;
            bool hardLimit = this.maxText.hard < this.text.Count;

            bool lifetimeLimit = VerbalAudioSource.maxLifetime < this.lifetime - this.text[last].timestamp;

            bool visible = this.text[last].mesh.fontSize > 0.001f;
            
            bool setToZero = this.text[last].size == 0f;

            //Debug.Log((softLimit, hardLimit, lifetimeLimit, visible, setToZero));

            switch ((softLimit, hardLimit, lifetimeLimit, visible, setToZero))
            {
                case (_, _, _, false, true):
                    //remove text element
                    GameObject.Destroy(this.text[last].mesh.gameObject);

                    this.text.RemoveAt(last);

                    break;
                case (_, true, _, true, false):
                case (true, false, true, true, false):
                    //set element to be deleted once size is 0
                    this.lastUpdateTimestamp = this.lifetime;
                    this.text[last] = (this.text[last].mesh, 0f, this.text[last].timestamp);
                    break;
                default:
                    //do nothing
                    break;
            }

            this.lifetime += Time.deltaTime;
        }
    }

    private float updateFontSize(float current, float goal, float duration)
    {

        float time = (this.lifetime - this.lastUpdateTimestamp) / duration;

        float value = this.fontSizeCurve.Evaluate(Mathf.Clamp(time, 0, 1));

        return Mathf.Lerp(current, goal, value);
    }
    private Vector3 updateTextPosition(Vector3 current, Vector3 goal, float duration)
    {
        float time = (this.lifetime - this.lastUpdateTimestamp) / duration;

        float value = this.textLocationCurve.Evaluate(Mathf.Clamp(time, 0, 1));

        return Vector3.Lerp(current, goal, value);
    }
    private Vector3 textPositionGoal(int index, Vector3 offset, Vector3 travelDirection)
    {
        Vector3 startPos = offset + Camera.main.WorldToScreenPoint(this.transform.position);
        return startPos + travelDirection * index;;
    }
    public void addText(string text)
    {
        GameObject tmp = VerbalAudioSource.createTextMesh(text);
        tmp.transform.parent = this.dialogCanvas.transform;
        tmp.transform.position = Camera.main.WorldToScreenPoint(this.transform.position);
        TextMeshProUGUI textMesh = tmp.GetComponent<TextMeshProUGUI>();

        textMesh.text = text;//could be replaced with an iterator that updates the text progamatically

        textMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
        textMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
        //textMesh.enableAutoSizing = true;
        textMesh.fontSize = 0;

        {
            RectTransform rect = textMesh.rectTransform;
            rect.sizeDelta = new Vector2(10 * text.Length / 2f, 10);
        }

        this.lastUpdateTimestamp = this.lifetime;
        this.text.Add((textMesh, 10, this.lifetime));
    }

    private static GameObject createTextMesh(string gameObjectName)
    {
        // progmatically initalize gameobject
        return new GameObject
        (
            gameObjectName,
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
    }
}
