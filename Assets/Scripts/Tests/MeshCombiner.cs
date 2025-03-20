using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class MeshCombiner : MonoBehaviour 
{
    [SerializeField] private MeshFilter[] sourceMeshFilters;
    
    void Start() {
        CombineMeshes();
    }

    void CombineMeshes() {
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        
        // 遍历所有需要合并的Mesh
        foreach (MeshFilter mf in sourceMeshFilters) {
            CombineInstance ci = new CombineInstance {
                mesh = mf.mesh,
                transform = mf.transform.localToWorldMatrix
            };
            combineInstances.Add(ci);
        }

        // 创建新Mesh并合并数据
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
        
        // 重新计算法线和包围盒
        combinedMesh.RecalculateNormals();
        combinedMesh.RecalculateBounds();

        // 设置到当前物体的MeshFilter
        MeshFilter thisMeshFilter = GetComponent<MeshFilter>();
        thisMeshFilter.mesh = combinedMesh;

        // 保持材质一致
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material = sourceMeshFilters[0].GetComponent<Renderer>().material;
    }
}