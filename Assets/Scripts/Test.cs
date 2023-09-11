using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    private string currentS = "ashsdaj";
    private string currentS2 = "sadaskldaa";
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        FindData();
    }
    public void FindData()
    {
        String[] currentS3 = { "zhuhai", "guangzhou", "shanghai1", "beijing", "tiananmen" };
        Point[] points = { new Point(100, 200),
            new Point(150, 250), new Point(250, 375),
            new Point(275, 395), new Point(295, 450) };
        var nowS = Array.Find(points, element => element.X * element.Y > 1000);
        Debug.Log(nowS);
        var tes = Array.Find(currentS3, element => element.Contains('b'));
        Debug.Log(tes);
    }
}
