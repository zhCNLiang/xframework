using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class TestSimpleJson : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var jsonNode = JSON.Parse("[{ \"key\" : \"a\", \"count\" : 10}, { \"key\" : \"a\", \"count\" : 10}, { \"key\" : \"a\", \"count\" : 10}, { \"key\" : \"a\", \"count\" : 10}]");
        foreach(var item in jsonNode.AsArray)
        {
            item.Value["rect"] = new Rect(0, 0, 100, 100);
            // Logger.Info?.Output(item.Value["key"].Value + " = " + item.Value["count"].AsInt);
        }

        // Debug.Log(jsonNode.ToString(4));
        var path = Application.dataPath + "/Scripts/Test/Arts/json.txt";
        jsonNode.SaveToBinaryFile(path);

        // // Logger.Info?.Output("===========================================");

        // jsonNode = JSONNode.LoadFromBinaryFile(path);

        // foreach(var item in jsonNode.AsArray)
        // {
        //     // Logger.Info?.Output(item.Value["key"].Value + " = " + item.Value["count"].AsInt);
        // }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
