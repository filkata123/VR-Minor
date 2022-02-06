using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    public float noiseStrength = 0.25f;

    private float objectHeight = 1.0f;
    private Material dissolveMaterial;

    private float secondsToWait = 3f;

    MeshCollider mesh_collider;
    Rigidbody rb;
    void Start()
    {
        mesh_collider = this.gameObject.GetComponent<MeshCollider>();
        rb = this.gameObject.GetComponent<Rigidbody>();

        //Start the coroutine we define below named ExampleCoroutine.
        StartCoroutine(FreezeObject());
    }
    IEnumerator FreezeObject()
    {
        //yield on a new YieldInstruction that waits for 3 seconds.
        yield return new WaitForSeconds(secondsToWait);

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        if (mesh_collider != null)
        {
            Destroy(mesh_collider);
        }

        dissolveMaterial = new Material(Shader.Find("Shader Graphs/Dissolve"));

        if (dissolveMaterial)
        {
            GetComponent<Renderer>().material = dissolveMaterial;
        }

        objectHeight += transform.position.y;

        StartCoroutine(DisolveObject());

    }
    IEnumerator DisolveObject()
    {
        SetHeight(objectHeight);
        yield return new WaitForEndOfFrame();
        objectHeight -= 0.01f;

        //TODO: Make better
        if(objectHeight < transform.position.y - 1)
        {
            Destroy(gameObject);
        }
        StartCoroutine(DisolveObject());
    }
    private void SetHeight(float height)
    {
        dissolveMaterial.SetFloat("_CutoffHeight", height);
        dissolveMaterial.SetFloat("_NoiseStrength", noiseStrength);
    }

}
