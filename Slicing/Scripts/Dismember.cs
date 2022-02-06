using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using EzySlice;

//TODO:
//1. Colour Insides: DONE - remove insides : DONE
//2. Cut only on high impact velocity: DONE
//3. Check direction of slice - dont slice with non-sharp part
//4. Newly created objects fade : DONE
//5. Make it cut when throwable (Empty object in place of hand?)
//6. Different Impact Velocity depending on Enemy?
//7. Enemies can be cut: DONE
//8. Cut Armature : DONE
//9. Duplication enemies (go through multiple collisions) : DONE
//10. FIX MESH AT ORIGIN BUG
public class Dismember : MonoBehaviour
{
    [Tooltip("How much is the minimum velocity to initiate slice")]
    public float sliceVelocity = 2f;
    [Tooltip("Should the body need more velocity than appendages for slice")]
    public bool bodyAppendageDifference = false;
    [Tooltip("How much is the minimum velocity to slice the main body when applicable")]
    public float bodySliceVelocity = 10f;

    [Tooltip("Whether newly created meshes by slicing should be cuttable")]
    public bool sliceCreatedCutMeshes = false;

    int sliceLayer = 11;

    private Transform hand;

    private Vector3 currentVelocity;
    private Vector3 prevVelocity;

    void Start()
    {
        prevVelocity = Vector3.zero;
    }

    void Update()
    {
        hand = this.GetComponentInParent<Transform>().gameObject.GetComponentInParent<Transform>();
        if (hand == null)
        {
            return;
        }
        CalculateHandSwingVelocity();
    }

    void DoSlice(GameObject obj, float velocity)
    {
        Material mat = null;

        if (bodyAppendageDifference)
        {
            if (obj.tag == "Body")
            {
                if(velocity < bodySliceVelocity)
                {
                    return;
                }
            }
        }

        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            mat = renderer.material;
        }
        else
        {
            SkinnedMeshRenderer sRenderer = obj.GetComponent<SkinnedMeshRenderer>();
            if(sRenderer!= null)
            {
                mat = obj.GetComponent<SkinnedMeshRenderer>().material;

            }
            else
            {
                obj.layer = sliceLayer;
                return;
            }
        }

        // If the object is the body, combine meshes and cut
        // else if object is an appendage add skinned mesh renderer to top of appendage and make bottom of appendage fall
        // else just create a simple cut (for debugging purposes)
        if(obj.tag == "Body")
        {
            StartCoroutine(CombineBodyPartsAndSlice(obj, mat));
        }
        else if (obj.tag == "Body_Appendix")
        {
            CutBodypart(obj, mat);
        }
        else
        {
            SlicedHull hull = SliceObject(obj, mat);
            if (hull != null)
            {
                GameObject top = hull.CreateUpperHull(obj, mat);
                GameObject bottom = hull.CreateLowerHull(obj, mat);
                AddSimpleHullComponent(bottom);
                AddSimpleHullComponent(top);
                Destroy(obj);
            }
        }

    }

    // Cut hand or leg
    void CutBodypart(GameObject obj, Material mat)
    {
        SlicedHull hull = SliceObject(obj, mat);
        if (hull != null)
        {
            GameObject top = hull.CreateUpperHull(obj, mat);
            GameObject bottom = hull.CreateLowerHull(obj, mat);
            AddSimpleHullComponent(bottom);
            AddComplexHullComponent(top, obj);
            top.transform.parent = obj.transform.parent;

        }
    }

    // Create a simply rendered component that dissolves after some seconds
    void AddSimpleHullComponent(GameObject new_obj)
    {
        if(sliceCreatedCutMeshes)
        {
            new_obj.layer = sliceLayer;
        }

        Rigidbody rb = new_obj.AddComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        MeshCollider collider = new_obj.AddComponent<MeshCollider>();
        collider.convex = true;

        new_obj.AddComponent<Dissolve>();
        
    }

    // Create a skinned mesh after cut
    void AddComplexHullComponent(GameObject new_obj, GameObject original_obj)
    {
        StartCoroutine(CreateSkinnedMesh(new_obj, original_obj));
        MeshCollider collider = new_obj.AddComponent<MeshCollider>();
        collider.convex = true;

    }

    // This function removes the new mesh renderer, waits for it to be destroyed (this happens at end of frame) and replaces it with a skinned mesh renderer
    // Then it takes the data from the original mesh renderer and applies it to the new one, which has the cut mesh
    // Finally it destroys the original object
    IEnumerator CreateSkinnedMesh(GameObject new_obj, GameObject original_obj)
    {
        new_obj.layer = sliceLayer;
        new_obj.tag = "Body_Appendix";

        Rigidbody rb = new_obj.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;

        Material mat = new_obj.GetComponent<MeshRenderer>().material;
        Destroy(new_obj.GetComponent<MeshRenderer>());

        yield return new WaitForEndOfFrame();

        if (original_obj != null)
        {
            SkinnedMeshRenderer SMR = original_obj.GetComponent<SkinnedMeshRenderer>();
            SkinnedMeshRenderer local_SMR = new_obj.AddComponent<SkinnedMeshRenderer>();

            local_SMR.material = mat;
            local_SMR.bones = SMR.bones;

            MeshFilter new_filter = new_obj.GetComponent<MeshFilter>();

            local_SMR.sharedMesh = new_filter.sharedMesh;
            local_SMR.sharedMesh.bindposes = SMR.sharedMesh.bindposes;

            local_SMR.sharedMesh.boneWeights = new_filter.sharedMesh.boneWeights;

            Destroy(original_obj);
        }
    }

    // Make a cut through all body parts but the main body and the armature and wait so that this syncs up with the other coroutine
    // Then get the mesh filters of all body parts and combine them in one mesh, to which a final cut is applied and all residual gameobjects are deleted
    IEnumerator CombineBodyPartsAndSlice(GameObject obj, Material mat)
    {
        List<MeshFilter> mesh_filters = new List<MeshFilter>();

        Transform main_parent = obj.transform.parent;

        for (int i = 0; i < main_parent.transform.childCount; i++)
        {
            GameObject child = main_parent.transform.GetChild(i).gameObject;
            if (child.name != "Armature" && child.tag != "Body")
            {
                CutBodypart(child, mat);
                yield return new WaitForEndOfFrame();
            }
        }

        for (int i = 0; i < main_parent.transform.childCount; i++)
        {
            GameObject child = main_parent.transform.GetChild(i).gameObject;
            if (child.name != "Armature")
            {
                MeshFilter child_mf = child.GetComponent<MeshFilter>();
                if (child_mf)
                {
                    mesh_filters.Add(child_mf);
                }
            }
        }

        CombineInstance[] combine = new CombineInstance[mesh_filters.Count];
        int j = 0;
        while (j < mesh_filters.Count)
        {
            combine[j].mesh = mesh_filters[j].sharedMesh;

            // The following matrix is applied to the newly created mesh. We need to create our own matrix as the normal localToWorld matrix does not correctly translate the position
            combine[j].transform = Matrix4x4.TRS(mesh_filters[j].transform.localPosition, mesh_filters[j].transform.rotation, mesh_filters[j].transform.localScale);
            mesh_filters[j].gameObject.SetActive(false);

            j++;
        }

        GameObject combined_object = new GameObject("Cut_Upper_Part");
        combined_object.transform.position = obj.transform.parent.position;

        combined_object.AddComponent<MeshRenderer>().material = mat;
        MeshFilter new_filter = combined_object.AddComponent<MeshFilter>();
        new_filter.mesh = new Mesh();
        new_filter.mesh.CombineMeshes(combine);

        Rigidbody rb = combined_object.AddComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        MeshCollider collider = combined_object.AddComponent<MeshCollider>();
        collider.convex = true;

        SlicedHull hull = SliceObject(combined_object, mat);
        if (hull != null)
        {
            GameObject bottom = hull.CreateLowerHull(combined_object, mat);
            GameObject top = hull.CreateUpperHull(combined_object, mat);

            AddSimpleHullComponent(bottom);
            AddSimpleHullComponent(top);
            Destroy(combined_object);
            Destroy(obj.transform.parent.gameObject);
        }
        else
        {
            //HACK, check IMPROVEMENT No1
            combined_object.layer = sliceLayer;
            Destroy(obj.transform.parent.gameObject);

        }
    }

    
    // Slice object
    SlicedHull SliceObject(GameObject obj, Material crossSectionmaterial = null)
    {
        // Check that slicing is not done with flat part of blade - THIS MAKES IT IMPOSSIBLE TO SLICE VERTICALLY, we probably need a different method
        // Around 0 == perpendicular
        //if (Vector3.Dot(hand.right, Vector3.down) < 0.5 && Vector3.Dot(hand.right, Vector3.down) > -0.5)
        //{
        //    return null;
        //}

        //IMPROVEMENT No1 = Don't get the current hand position, but the position during first contact

        // Get up position no matter whether the slice is done with a backhand slash or normal slash
        // Almost 1 == parallel
        if (Vector3.Dot(hand.right, Vector3.down) > 0 )
        {
            return obj.Slice(hand.position, -hand.right, crossSectionmaterial);
        }
        // Almost -1 == inversed parallel
        return obj.Slice(hand.position, hand.right, crossSectionmaterial);
    }

    void CalculateHandSwingVelocity()
    {
        currentVelocity = (hand.position - prevVelocity) / Time.deltaTime;

        prevVelocity = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == sliceLayer)
        {
            if (currentVelocity.magnitude > sliceVelocity)
            {
                ContactPoint[] contact_points = new ContactPoint[collision.contacts.Length];
                collision.GetContacts(contact_points);

                for (int i = 0; i < contact_points.Length; i++)
                {
                    //make sure that only the "Blade" collider is used
                    if(contact_points[i].thisCollider.gameObject.name != "Blade")
                    {
                        return;
                    }
                }

                DoSlice(collision.gameObject, currentVelocity.magnitude);
            }
        }
    }
}

