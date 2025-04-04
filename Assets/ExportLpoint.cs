using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class ExportLpoint : MonoBehaviour
{
    // Start is called before the first frame update

    public GraphDisplay graphDisplay;
    public GeneralStatUtils gen_data;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        ListPoint points = graphDisplay._graph.getLPoints();
        string path = gen_data.workingPath + "/graph" + graphDisplay.courbeType.value + ".csv";
        System.IO.StreamWriter file = new System.IO.StreamWriter(path);
        file.WriteLine("x;y");
        foreach (Vector2d point in points.getListPoint())
        {
            file.WriteLine(point.x + ";" + point.y);
        }
        file.Close();
    }

}
