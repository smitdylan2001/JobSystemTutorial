using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshManager : MonoBehaviour
{
    //General
    [SerializeField] Vector3 rippleOrigin;

    Mesh mesh;

    //Regular
    Vector3[] verticesArray, newVerticesArray;

    private void Start()
    {
        //Setup general data
        mesh = GetComponent<MeshFilter>().mesh;

        //Setup regular deform data
        verticesArray = mesh.vertices;
        newVerticesArray = new Vector3[mesh.vertexCount];
    }

    private void Update()
    {
        for (int i = 0; i < verticesArray.Length; i++)
        {
            Vector3 originalVertex = verticesArray[i];
            float distance = Vector3.Distance(originalVertex, rippleOrigin);
            float rippleAmount = Mathf.Sin(distance - Time.time);
            Vector3 offset = (originalVertex - rippleOrigin).normalized * rippleAmount;
            Vector3 newPos = originalVertex + offset;

            newVerticesArray[i] = newPos;
        }
        mesh.SetVertices(newVerticesArray);
    }
}
