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
        
        // ����������Ҫ�ϲ���Mesh
        foreach (MeshFilter mf in sourceMeshFilters) {
            CombineInstance ci = new CombineInstance {
                mesh = mf.mesh,
                transform = mf.transform.localToWorldMatrix
            };
            combineInstances.Add(ci);
        }

        // ������Mesh���ϲ�����
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
        
        // ���¼��㷨�ߺͰ�Χ��
        combinedMesh.RecalculateNormals();
        combinedMesh.RecalculateBounds();

        // ���õ���ǰ�����MeshFilter
        MeshFilter thisMeshFilter = GetComponent<MeshFilter>();
        thisMeshFilter.mesh = combinedMesh;

        // ���ֲ���һ��
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material = sourceMeshFilters[0].GetComponent<Renderer>().material;
    }
}