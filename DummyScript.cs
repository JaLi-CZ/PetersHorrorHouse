#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

[ExecuteInEditMode]
public class DummyScript : MonoBehaviour
{
    public Material goldenKeyMat, normalGlowMat, redGlowMat, goldenFrameMat, whiteCeramicMat, metalMat, darkMetalMat;
    public GameObject pointLight;

    public bool run = false;
    public bool addMeshCollider = false;
    public bool addCollidersToOpenable = false;
    public bool makeCollidersNavigationStatic = false;
    public bool addBoxCollidersToKeys = false;
    public bool addBoxCollidersToPianoKeys = false;
    public bool setMaterials = false;
    public bool addAudioSources = false;

    private int openableLayer;

    void Start()
    {
        if (!run) return;

        openableLayer = LayerMask.NameToLayer("Openable");

        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if(addAudioSources)
            {
                if (obj.name.Contains("Guillotine Cutter") || obj.name.Contains("OpeningWheel") || obj.name.Equals("END_DOORS_COLLIDER_IRON") || 
                obj.name == "COLLIDER_Clown" && obj.GetComponent<AudioSource>() == null)
                {
                    obj.AddComponent<AudioSource>();
                }
            }
            if (obj.name.Contains("COLLIDER"))
            {
                if (addMeshCollider)
                {
                    if (addMeshCollider && obj.GetComponent<MeshCollider>() == null) obj.AddComponent<MeshCollider>();
                }
                if(makeCollidersNavigationStatic)
                {
                    GameObjectUtility.SetStaticEditorFlags(obj, StaticEditorFlags.NavigationStatic);
                }
            }
            else if(addCollidersToOpenable && obj.name.Contains("OPEN"))
            {
                obj.layer = openableLayer;
                if (obj.GetComponent<MeshCollider>() == null) obj.AddComponent<MeshCollider>();
                if(addAudioSources) {
                    AudioSource audioSource = obj.GetComponent<AudioSource>();
                    if(audioSource == null) audioSource = obj.AddComponent<AudioSource>();
                    audioSource.spatialBlend = 1f;
                    audioSource.maxDistance = 7f;
                    audioSource.spread = 1.5f;
                }
            }
            else if(addBoxCollidersToPianoKeys && obj.name.Contains("PIANO_KEY")) {
                BoxCollider boxCollider = obj.GetComponent<BoxCollider>();
                if (boxCollider == null) boxCollider = obj.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
            }

            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (obj.name.StartsWith("KEY_"))
                {
                    if(setMaterials) renderer.sharedMaterial = goldenKeyMat;
                    if(addBoxCollidersToKeys)
                    {
                        BoxCollider boxCollider;
                        while((boxCollider = obj.GetComponent<BoxCollider>()) != null) DestroyImmediate(boxCollider);
                        boxCollider = obj.AddComponent<BoxCollider>();
                        boxCollider.isTrigger = true;
                        boxCollider.size = new Vector3(boxCollider.size.x * 1.4f, boxCollider.size.y, boxCollider.size.z * 2.1f);
                    }
                }
                else if (obj.name.Contains("RED_GLOW"))
                {
                    if (setMaterials) renderer.sharedMaterial = redGlowMat;
                }
                else if (obj.name.Contains("GLOW"))
                {
                    if (setMaterials) renderer.sharedMaterial = normalGlowMat;
                }
                else if (obj.name.EndsWith("Frame"))
                {
                    if (setMaterials) renderer.sharedMaterial = goldenFrameMat;
                }
                else if (obj.name.EndsWith("WHITE_CERAMIC"))
                {
                    if (setMaterials) renderer.sharedMaterial = whiteCeramicMat;
                }
                else if (obj.name.Contains("DARK_IRON") || obj.name.Contains("Briefcase"))
                {
                    if (setMaterials) renderer.sharedMaterial = darkMetalMat;
                }
                else if (obj.name.Contains("IRON"))
                {
                    if (setMaterials) renderer.sharedMaterial = metalMat;
                }
            }
        }
    }

    void Update()
    {
        if (!run) return;

        Start();
    }
}
#endif