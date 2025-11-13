using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class IsoTileTest : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 20;
    public int height = 20;
    public float cellSize = 1f;
    public float zOffset = 0.001f; // 겹침 방지용 오프셋

    [Header("Visuals")]
    public Material tileMaterial;   // X자 스프라이트 머티리얼
    public float offsetY;

  void Start()
{
    Mesh mesh = BuildRectGrid(width, height, cellSize, zOffset);

    var mf = GetComponent<MeshFilter>();
    mf.mesh = mesh;

    var mr = GetComponent<MeshRenderer>();
    mr.material = tileMaterial;

    // 중앙 오프셋 계산
    float half = cellSize * 0.5f;
    float rectWorldX = width * half;
    float rectWorldY = width * half;
    
    transform.position = new Vector3(-rectWorldX,-40f, -rectWorldY + offsetY);

    //float worldX = ((width - 1) - (height - 1)) * half * 0.5f;
    //float worldZ = ((width - 1) + (height - 1)) * half * 0.5f;

    // 그리드 중앙을 기준으로 이동
    //transform.position = new Vector3(-worldX, 0, -worldZ);
}

    //사각형
    Mesh BuildRectGrid(int width, int height, float cellSize, float zOffset)
    {
        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        int vIndex = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float worldX = x * cellSize;
                float worldZ = y * cellSize;

                Vector3 basePos = new Vector3(worldX, 0, worldZ + zOffset * (x + y));

                // 정사각형 4꼭짓점
                vertices.Add(basePos + new Vector3(0, 0, cellSize));
                vertices.Add(basePos + new Vector3(cellSize, 0, cellSize));
                vertices.Add(basePos + new Vector3(cellSize, 0, 0));
                vertices.Add(basePos + new Vector3(0, 0, 0));

                // UV 좌표 (스프라이트 그대로 매핑)
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 1));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(0, 0));

                // 삼각형 2개
                triangles.Add(vIndex + 0);
                triangles.Add(vIndex + 1);
                triangles.Add(vIndex + 2);

                triangles.Add(vIndex + 2);
                triangles.Add(vIndex + 3);
                triangles.Add(vIndex + 0);

                vIndex += 4;
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }


    //다이아몬드
    Mesh BuildIsoGrid(int width, int height, float cellSize, float zOffset)
    {
        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        int vIndex = 0;
        float half = cellSize * 0.5f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 아이소매트릭 변환 (다이아몬드 위치 계산)
                float worldX = (x - y) * half;
                float worldZ = (x + y) * half;

                Vector3 basePos = new Vector3(worldX, 0, worldZ + zOffset * (x + y));

                // 다이아몬드 4점 (UV는 [0~1]로 기본 세팅)
                vertices.Add(basePos + new Vector3(0, 0, half));
                vertices.Add(basePos + new Vector3(half, 0, 0));
                vertices.Add(basePos + new Vector3(0, 0, -half));
                vertices.Add(basePos + new Vector3(-half, 0, 0));

                uvs.Add(new Vector2(0.5f, 1f));  // 위
                uvs.Add(new Vector2(1f, 0.5f));  // 오른쪽
                uvs.Add(new Vector2(0.5f, 0f));  // 아래
                uvs.Add(new Vector2(0f, 0.5f));  // 왼쪽

                // 삼각형 두 개
                triangles.Add(vIndex + 0);
                triangles.Add(vIndex + 1);
                triangles.Add(vIndex + 2);

                triangles.Add(vIndex + 2);
                triangles.Add(vIndex + 3);
                triangles.Add(vIndex + 0);

                vIndex += 4;
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}
