using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyGrid : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [Tooltip("Length of one cell in Unity units")]
    [SerializeField] private int cellSize;
    [Tooltip("Amount of vertices per length of a cell")]
    [SerializeField] private int meshDetail;
    [SerializeField] private int pixelAccuracy;

    [SerializeField,Range(0,0.3f)] private float cellTypeNoiseScale;
    [SerializeField, Range(0, 0.3f)] private float heightMapNoiseScale;
    [SerializeField] private float maxHeight;

    //[SerializeField] private RawImage image;

    private Cell[,] grid;
    private float[,] cellTypeNoiseMap;
    private float[,] heightNoiseMap;



    private void Start()
    {
        //Generate();
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            Generate();
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) { return; }
        if (cellTypeNoiseMap == null || cellTypeNoiseMap.Length < 1) { return; }


        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (cellTypeNoiseMap[i, j] <= 0.2) { Gizmos.color = Color.green; }
                else if (cellTypeNoiseMap[i, j] <= 0.4) { Gizmos.color = Color.black; }
                else if (cellTypeNoiseMap[i, j] <= 0.6) { Gizmos.color = Color.yellow; }
                else if (cellTypeNoiseMap[i, j] <= 0.8) { Gizmos.color = Color.white; }
                else if (cellTypeNoiseMap[i, j] <= 1.0) { Gizmos.color = Color.blue; }
                Vector3 pos = new Vector3(i, 0, j);
                Gizmos.DrawCube(pos, Vector3.one);
            }
        }
    }

    public void Generate()
    {
        grid = new Cell[width, height];
        cellTypeNoiseMap = new float[width, height];
        heightNoiseMap = new float[width*meshDetail+1, height*meshDetail+1];

        Mesh mesh = new Mesh();
        Texture2D cellTypeTexture = new Texture2D(width * meshDetail * pixelAccuracy, height * meshDetail * pixelAccuracy, UnityEngine.Experimental.Rendering.GraphicsFormat.R16_SFloat, UnityEngine.Experimental.Rendering.TextureCreationFlags.DontInitializePixels);

        int pixelsPerCell = meshDetail * pixelAccuracy;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float noiseValue = Mathf.Clamp(Mathf.PerlinNoise((float)i * cellTypeNoiseScale, (float)j * cellTypeNoiseScale), 0, 1);
                cellTypeNoiseMap[i, j] = noiseValue;

                for (int k = 0; k < pixelsPerCell; k++)
                {
                    for (int l = 0; l < pixelsPerCell; l++)
                    {
                        cellTypeTexture.SetPixel(i * pixelsPerCell + k, j * pixelsPerCell + l, new Color(noiseValue, 0, 0));
                    }
                }
                //Debug.Log(noiseValue);

                grid[i, j] = new Cell(cellSize, meshDetail, GridHelper.GetCellTypePerlinNoise(heightNoiseMap[i, j]));
            }
        }
        cellTypeTexture.Apply();

        Debug.Log(meshDetail);
        mesh = GridHelper.GeneratePerformantGridMesh(width, height, meshDetail, ref heightNoiseMap, heightMapNoiseScale, maxHeight, 0);

        GetComponent<MeshFilter>().mesh = mesh;
        //GetComponent<MeshFilter>().mesh.RecalculateUVDistributionMetric(0);
        //GetComponent<MeshFilter>().mesh.RecalculateNormals(0);

        Debug.Log(cellTypeTexture.GetPixel(0,0));
        GetComponent<MeshRenderer>().material.SetTexture("_CellTypeTexture", cellTypeTexture);



        //image.texture = cellTypeTexture;
    }

    private void GenerateHeightNoiseMap()
    {

    }
}
