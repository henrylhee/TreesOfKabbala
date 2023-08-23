using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class GridHelper
{
    public static CellTypes GetCellTypeRandom()
    {
        float value = UnityEngine.Random.Range(0f, 1f);
        if (value <= 0.2) 
        { 
            return CellTypes.STONE; 
        }
        else if (value <= 0.4) { return CellTypes.GRASS; }
        else if (value <= 0.6) { return CellTypes.ENERGY; }
        else if (value <= 0.8) { return CellTypes.EARTH; }
        else { return CellTypes.WATER; }
    }
    public static CellTypes GetCellTypePerlinNoise(float noise)
    {
        if (noise <= 0.2) { return CellTypes.STONE; }
        else if (noise <= 0.4) { return CellTypes.GRASS; }
        else if (noise <= 0.6) { return CellTypes.ENERGY; }
        else if (noise <= 0.8) { return CellTypes.EARTH; }
        else { return CellTypes.WATER; }
    }

    public static Mesh GenerateGridMesh(int width, int height, int cellDetail, ref float[,] heightMap, float heightMapNoiseScale, 
                                        float maxHeight, float nullHeight)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(width*cellDetail+1)*(height*cellDetail+1)];
        int[] indices = new int[width * height * cellDetail * cellDetail * 6];
        Vector2[] uvs = new Vector2[(width * cellDetail + 1) * (height * cellDetail + 1)];


        float startX = -(width / 2);
        float startY = -(height / 2);
        float widthStep = 1f / cellDetail;
        float heigthStep = 1f / cellDetail;


        int k = 0;

        for (int i = 0; i < width*cellDetail + 1; i++)
        {
            for (int j = 0; j < height*cellDetail + 1; j++)
            {
                float noiseValue = Mathf.Clamp(Mathf.PerlinNoise((float)i * heightMapNoiseScale, (float)j * heightMapNoiseScale), 0, 1);
                heightMap[i,j] = noiseValue;

                vertices[k] = new Vector3(startX + i*widthStep, nullHeight + (noiseValue * maxHeight), startY + j*heigthStep);
                uvs[k] = new Vector2((float)i/(width*cellDetail), (float)j /(height*cellDetail));

                k++;
            }
        }

        k = 0;
        for (int i = 0; i < width * cellDetail; i++)
        {            
            for (int j = 0; j < height * cellDetail; j++)
            {
                indices[k] = (width * cellDetail+1) * i + j;
                indices[k+1] = (width * cellDetail+1) * i + j+1;
                indices[k+2] = (width * cellDetail+1) * (i+1) + j;
                indices[k+3] = (width * cellDetail+1) * (i + 1) + j;
                indices[k+4] = (width * cellDetail+1) * (i) + j+1;
                indices[k+5] = (width * cellDetail+1) * (i + 1) + j+1;

                k += 6;
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.SetUVs(0, uvs);


        return mesh;
    }

    public static Mesh GeneratePerformantGridMesh(int width, int height, int cellDetail, ref float[,] heightMap, float heightMapNoiseScale,
                                        float maxHeight, float nullHeight)
    {
        int vertexAttributeCount = 3;

        int vertexCount = (width * cellDetail + 1) * (height * cellDetail + 1);
        int triangleIndexCount = 6 * width * cellDetail * height * cellDetail;

        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
        vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3, stream: 1);
        vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2, stream: 2);

        meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
        vertexAttributes.Dispose();

        meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt16);

        NativeArray<float3> vertices = meshData.GetVertexData<float3>();
        NativeArray<float3> normals = meshData.GetVertexData<float3>(1);
        //NativeArray<float4> tangents = meshData.GetVertexData<float4>(2);
        NativeArray<float2> texCoords = meshData.GetVertexData<float2>(2);

        NativeArray<ushort> triangleIndices = meshData.GetIndexData<ushort>();



        float startX = -(width / 2);
        float startY = -(height / 2);
        float widthStep = 1f / cellDetail;
        float heigthStep = 1f / cellDetail;


        int k = 0;

        for (int i = 0; i < width * cellDetail + 1; i++)
        {
            for (int j = 0; j < height * cellDetail + 1; j++)
            {
                float noiseValue = Mathf.Clamp(Mathf.PerlinNoise((float)i * heightMapNoiseScale, (float)j * heightMapNoiseScale), 0, 1);
                heightMap[i, j] = noiseValue;

                vertices[k] = new Vector3(startX + i * widthStep, nullHeight + (noiseValue * maxHeight), startY + j * heigthStep);
                texCoords[k] = new Vector2((float)i / (width * cellDetail), (float)j / (height * cellDetail));

                k++;
            }
        }

        k = 0;
        Vector3 top;
        Vector3 bot;
        Vector3 right;
        Vector3 left;
        Vector3 normal;

        //note: leaving out 2 tringles for faster computation
        //crossproduct if triangles not normalized right now
        for (int i = 0; i < width * cellDetail + 1; i++)
        {
            for (int j = 0; j < height * cellDetail + 1; j++)
            {
                if(i > 0 && i < width * cellDetail && j > 0 && j < height * cellDetail)
                {
                    normal = new Vector3(0,0,0);
                    top = vertices[k + 1];
                    bot = vertices[k - 1];
                    right = vertices[k + height * cellDetail + 1];
                    left = vertices[k - height * cellDetail + 1];

                    normal = normal + Vector3.Cross(bot, left);
                    normal = normal + Vector3.Cross(left, top);
                    normal = normal + Vector3.Cross(top, right);
                    normal = normal + Vector3.Cross(right, bot);

                    normals[k] = normal.normalized;
                }
                else if (i > 0 && i < width * cellDetail && j == 0)
                {
                    normal = new Vector3(0, 0, 0);
                    top = vertices[k + 1];
                    right = vertices[k + height * cellDetail + 1];
                    left = vertices[k - height * cellDetail + 1];

                    normal = normal + Vector3.Cross(left, top);
                    normal = normal + Vector3.Cross(top, right);

                    normals[k] = normal.normalized;
                }
                else if (i > 0 && i < width * cellDetail && j == height * cellDetail)
                {
                    normal = new Vector3(0, 0, 0);
                    bot = vertices[k - 1];
                    right = vertices[k + height * cellDetail + 1];
                    left = vertices[k - height * cellDetail + 1];

                    normal = normal + Vector3.Cross(bot, left);
                    normal = normal + Vector3.Cross(right, bot);

                    normals[k] = normal.normalized;
                }
                else if (i == 0 && j > 0 && j < height * cellDetail)
                {
                    normal = new Vector3(0, 0, 0);
                    top = vertices[k + 1];
                    bot = vertices[k - 1];
                    right = vertices[k + height * cellDetail + 1];

                    normal = normal + Vector3.Cross(top, right);
                    normal = normal + Vector3.Cross(right, bot);

                    normals[k] = normal.normalized;
                }
                else if (i == width * cellDetail && j > 0 && j < height * cellDetail)
                {
                    normal = new Vector3(0, 0, 0);
                    top = vertices[k + 1];
                    bot = vertices[k - 1];
                    left = vertices[k - height * cellDetail + 1];

                    normal = normal + Vector3.Cross(bot, left);
                    normal = normal + Vector3.Cross(left, top);

                    normals[k] = normal.normalized;
                }
                else if (i == 0 && j == 0)
                {
                    normal = new Vector3(0, 0, 0);
                    top = vertices[k + 1];
                    right = vertices[k + height * cellDetail + 1];

                    normal = normal + Vector3.Cross(top, right);

                    normals[k] = normal.normalized;
                }
                else if (i == 0 && j == height * cellDetail)
                {
                    normal = new Vector3(0, 0, 0);
                    bot = vertices[k - 1];
                    right = vertices[k + height * cellDetail + 1];

                    normal = normal + Vector3.Cross(right, bot);

                    normals[k] = normal.normalized;
                }
                else if (i == width * cellDetail && j == height * cellDetail)
                {
                    normal = new Vector3(0, 0, 0);
                    bot = vertices[k - 1];
                    left = vertices[k - height * cellDetail + 1];

                    normal = normal + Vector3.Cross(bot, left);

                    normals[k] = normal.normalized;
                }
                else if (i == width * cellDetail && j == 0)
                {
                    normal = new Vector3(0, 0, 0);
                    top = vertices[k + 1];
                    left = vertices[k - height * cellDetail + 1];

                    normal = normal + Vector3.Cross(left, top);

                    normals[k] = normal.normalized;
                }

                k++;
            }
        }

        k = 0;

        for (int i = 0; i < width * cellDetail; i++)
        {
            for (int j = 0; j < height * cellDetail; j++)
            {
                triangleIndices[k] = (ushort)((width * cellDetail + 1) * i + j);
                triangleIndices[k + 1] = (ushort)((width * cellDetail + 1) * i + j + 1);
                triangleIndices[k + 2] = (ushort)((width * cellDetail + 1) * (i + 1) + j);
                triangleIndices[k + 3] = (ushort)((width * cellDetail + 1) * (i + 1) + j);
                triangleIndices[k + 4] = (ushort)((width * cellDetail + 1) * (i) + j + 1);
                triangleIndices[k + 5] = (ushort)((width * cellDetail + 1) * (i + 1) + j + 1);

                k += 6;
            }
        }


        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndexCount));

        var mesh = new Mesh { name = "Procedural Mesh" };
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);

        return mesh;
    }


}
