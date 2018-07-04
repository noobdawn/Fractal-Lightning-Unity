using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LightningSegment
{
    public Vector3 start;
    public Vector3 end;
    public LightningSegment LeftChild;
    public LightningSegment RightChild;
    public LightningSegment Branch;
    public float uv;
    public int fractal;
    public LightningSegment(Vector3 s, Vector3 e)
    {
        start = s;
        end = e;
        LeftChild = null;
        RightChild = null;
        Branch = null;
        uv = 0.5f;
        fractal = 1;
    }
    public bool isLeaf()
    {
        return LeftChild == null && RightChild == null;
    }
    public Vector3 Middle()
    {
        return (start + end) * 0.5f;
    }
    public void Draw()
    {
        if (isLeaf())
        {
            Gizmos.DrawLine(start, end);
        }
        else
        {
            LeftChild.Draw();
            RightChild.Draw();
            if (Branch != null)
                Branch.Draw();
        }
    }
}

public class LightningCreator : MonoBehaviour {
    private static float[] arrayForUV = new float[]{
        1f, 0.5f, 0.25f, 0.125f, 0.0625f, 0.03125f,
        0.015625f, 0.0078125f, 0.00390625f, 0.001953125f,
        0.0009765625f, 0.00048828125f
    };
    LightningSegment trunk;
    public float BaseAttenuation = 1f;
    public float MaxOffset = 0.1f;
    public float MinOffset = -0.1f;
    public int FractalTime = 10;
    public float BranchChance = 0.1f;
    public float Radius = 0.01f;
    public float RadiusAttenuation = 0.7f;
    public Transform Observer;

    public GameObject templet;

    public float LightningChance = 0.5f;
    void Update()
    {
        if (Random.Range(0,1f) > LightningChance)
        {
            Vector3 start = new Vector3( Random.Range(-10f, 10f), 10f, 0);
            Vector3 end = new Vector3(Random.Range(-2f, 2f) + start.x, -6.5f, 0);
            CreateLightning(
                start,
                end
                );
        }
    }

    public void CreateLightning(Vector3 start, Vector3 end)
    {
        if (start == end) return;
        float Length = (end - start).magnitude;
        trunk = new LightningSegment(start, end);
        Vector2 OffsetRange = Length * new Vector2(MinOffset, MaxOffset);
        //计算一个正交基
        Vector3 LightningDir = end - start;
        //找到不为0的一个分量
        Vector3 forward;
        if (LightningDir.x != 0)
        {
            forward = new Vector3(-(LightningDir.y + LightningDir.z) / LightningDir.x, 1, 1);
        }
        else if (LightningDir.y != 0)
        {
            forward = new Vector3(1, -(LightningDir.x + LightningDir.z) / LightningDir.y, 1);
        }
        else if (LightningDir.z != 0)
        {
            forward = new Vector3(1, 1, -(LightningDir.x + LightningDir.y) / LightningDir.z);
        }
        else
            return;
        forward.Normalize();
        Vector3 right = Vector3.Cross(LightningDir, forward);
        right.Normalize();
        LightningDir.Normalize();
        GetFractcalLightning(trunk, 1, FractalTime, BaseAttenuation, OffsetRange, LightningDir, forward, right, BranchChance);
        Mesh mesh = MakeMesh(trunk, Radius, RadiusAttenuation, Observer.position, Length);
        //创造新的闪电
        GameObject go = Instantiate(templet) as GameObject;
        go.GetComponent<MeshFilter>().mesh = mesh;
        go.transform.position = Vector3.zero;
        go.transform.parent = transform;
        go.SetActive(true);
    }

    /// <summary>
    /// 生成路径的递归函数
    /// </summary>
    /// <param name="lightningSegment">需要划分的线段</param>
    /// <param name="fractalTime">当前递归次数</param>
    /// <param name="maxFractalTime">最大递归次数</param>
    /// <param name="baseAttenuation">基础衰减</param>
    /// <param name="offsetRange">偏离值的范围</param>
    /// <param name="direction">闪电方向</param>
    /// <param name="forward">与闪电方向垂直的一个单位向量</param>
    /// <param name="right">与闪电方向垂直的一个单位向量</param>
    /// <param name="branchChance">生成分支的几率</param>
    private void GetFractcalLightning(LightningSegment lightningSegment, int fractalTime, int maxFractalTime, float baseAttenuation, Vector2 offsetRange, Vector3 direction, Vector3 forward, Vector3 right, float branchChance = 0f)
    {
        if (!lightningSegment.isLeaf() || fractalTime > maxFractalTime)
            return;
        float zOffset = Mathf.Exp(-baseAttenuation * fractalTime) * Random.Range(offsetRange.x, offsetRange.y);
        float xOffset = Mathf.Exp(-baseAttenuation * fractalTime) * Random.Range(offsetRange.x, offsetRange.y);
        Vector3 mid = lightningSegment.Middle() + xOffset * right + zOffset * forward;
        lightningSegment.LeftChild = new LightningSegment(lightningSegment.start, mid);
        lightningSegment.LeftChild.fractal = fractalTime + 1;
        lightningSegment.LeftChild.uv = lightningSegment.uv - LightningCreator.arrayForUV[lightningSegment.LeftChild.fractal];
        lightningSegment.RightChild = new LightningSegment(mid, lightningSegment.end);
        lightningSegment.RightChild.fractal = fractalTime + 1;
        lightningSegment.RightChild.uv = lightningSegment.uv + LightningCreator.arrayForUV[lightningSegment.RightChild.fractal];
        GetFractcalLightning(lightningSegment.LeftChild, fractalTime + 1, maxFractalTime, baseAttenuation, offsetRange, direction, forward, right, branchChance);
        GetFractcalLightning(lightningSegment.RightChild, fractalTime + 1, maxFractalTime, baseAttenuation, offsetRange, direction, forward, right, branchChance);
        if (Random.Range(0f, 1f) < branchChance)
        {
            //制作h形片段
            zOffset = Mathf.Exp(-baseAttenuation * fractalTime) * Random.Range(offsetRange.x, offsetRange.y);
            xOffset = Mathf.Exp(-baseAttenuation * fractalTime) * Random.Range(offsetRange.x, offsetRange.y);
            float yOffset = Mathf.Exp(-baseAttenuation * fractalTime) * Random.Range(offsetRange.x, offsetRange.y);
            Vector3 branchEnd = lightningSegment.Middle() + xOffset * right + zOffset * forward + yOffset * direction;
            lightningSegment.Branch = new LightningSegment(mid, branchEnd);
            lightningSegment.Branch.fractal = fractalTime + 1;
            lightningSegment.Branch.uv = lightningSegment.uv + LightningCreator.arrayForUV[lightningSegment.Branch.fractal];
            GetFractcalLightning(lightningSegment.Branch, fractalTime + 1, maxFractalTime, baseAttenuation, offsetRange, direction, forward, right, branchChance);
        }
    }

    private Mesh MakeMesh(LightningSegment lightningSegment, float radius, float attenuation, Vector3 obPos, float length)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int idx = 0;
        SegmentToMesh(lightningSegment, vertices, triangles, uvs, obPos, radius, attenuation, ref idx);
        mesh.SetVertices(vertices);
        mesh.triangles = triangles.ToArray();
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        return mesh;
    }

    /// <summary>
    /// 填充Mesh的递归函数
    /// </summary>
    /// <param name="lightningSegment">需要生成Mesh的线段</param>
    /// <param name="vertices"></param>
    /// <param name="triangle"></param>
    /// <param name="uv"></param>
    /// <param name="obPos">摄像机位置</param>
    /// <param name="radius">当前的闪电粗细</param>
    /// <param name="attenuation">分支闪电的衰减程度</param>
    /// <param name="idx">顶点序号</param>
    private void SegmentToMesh(LightningSegment lightningSegment, List<Vector3> vertices, List<int> triangle,
        List<Vector2> uv, Vector3 obPos, float radius, float attenuation, ref int idx)
    {
        if (lightningSegment.isLeaf())
        {
            Vector3 zMid = lightningSegment.Middle();
            Vector3 zNormal = Vector3.Cross(zMid - obPos, lightningSegment.end - lightningSegment.start).normalized;
            var finalR = radius * attenuation;
            if (lightningSegment.Branch == null)
            {
                //Z型片段的Mesh生成
                vertices.Add(lightningSegment.start + finalR * zNormal);
                vertices.Add(lightningSegment.start - finalR * zNormal);
                vertices.Add(lightningSegment.end + finalR * zNormal);
                vertices.Add(lightningSegment.end - finalR * zNormal);
                triangle.Add(idx + 1);
                triangle.Add(idx);
                triangle.Add(idx + 2);
                triangle.Add(idx + 1);
                triangle.Add(idx + 2);
                triangle.Add(idx + 3);
                uv.Add(new Vector2(1, lightningSegment.uv));
                uv.Add(new Vector2(0, lightningSegment.uv));
                uv.Add(new Vector2(1, lightningSegment.uv));
                uv.Add(new Vector2(0, lightningSegment.uv));
                idx += 4;
            }
        }
        else
        {
            SegmentToMesh(lightningSegment.LeftChild, vertices, triangle, uv, obPos, radius, attenuation, ref idx);
            SegmentToMesh(lightningSegment.RightChild, vertices, triangle, uv, obPos, radius, attenuation, ref idx);
            if (lightningSegment.Branch != null)
                SegmentToMesh(lightningSegment.Branch, vertices, triangle, uv, obPos, radius * attenuation, attenuation, ref idx);
        }
    }
}
