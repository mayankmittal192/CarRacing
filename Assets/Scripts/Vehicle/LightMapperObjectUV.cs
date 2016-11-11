using UnityEngine;


[ExecuteInEditMode]
public class LightMapperObjectUV : MonoBehaviour
{
    public float fudgeScale = 1.0f;
    public Transform theObject;
    public Vector3 terrainSize = new Vector3(1450, 1450, -1450); // inverse the z here

    void Start()
    {
        if (!theObject)
            theObject = transform;
    }

    void OnRenderObject()
    {
        Vector3 inverseScale = new Vector3(1.0f / terrainSize.x, 1.0f / terrainSize.y, 1.0f / terrainSize.z);
        Matrix4x4 uvMat = Matrix4x4.TRS(Vector3.Scale(new Vector3(theObject.position.x, theObject.position.z, theObject.position.y), inverseScale), 
            Quaternion.Euler(90, 0, 0) * Quaternion.Inverse(theObject.rotation), inverseScale * fudgeScale);
        Shader.SetGlobalMatrix("_LightmapMatrix", uvMat);
    }
}
