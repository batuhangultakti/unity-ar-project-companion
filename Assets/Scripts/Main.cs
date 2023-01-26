using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]

//COLLECTED ALL CODE IN A SINGE MAIN FOLDER TO ACCESS THE DATA EASIER SINCE PROJECT SCOPE IS SMALL
public class Main : MonoBehaviour
{
    //ANIMATOR FOR DUCKY
    Animator animator; 

    //CURSOR AND PREFAB DUCKY
    public GameObject gameObjectToInstantiate;
    public GameObject indicator;

    //UI LOGIC
    public GameObject CTA1;
    public GameObject CTA2;
    public GameObject ButtonsUI;

    //SPAWNED DUCKY
    private GameObject spawnedObject;

    //LOGICS WE IMPORT
    private ARRaycastManager _arRaycastManager;
    private ARPlaneManager _arPlaneManager;
    private Vector2 touchPosition;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    //DOUBLE TAP AND DELAY
    private bool experinceStarted = false;
    private float lastTimeTapped;
    private bool EasterEggFound = false;

    //INFO ABOUT THE SPAWNED DUCK
    private GameObject duckHat;
    private GameObject duckBody;
    private GameObject mainDuck;
    private Renderer ren;
    private Material[] mat;
    //MATERIALS FOR DUCKY
    public string currentMaterial = "yellow";
    public Material magenta;
    public Material cyan;
    public Material yellow;

    private void Awake()
    {
        StartCoroutine(CTA1FadeOut());
        _arRaycastManager = GetComponent<ARRaycastManager>();
        _arPlaneManager = GetComponent<ARPlaneManager>();

    }

    //INITIAL WAIT FOR CTA CHANGE AND SET UP OF THE PLANES
    //USED WAIT TO BE REALLY SURE TO SCAN THE AREA SINCE ON ONLY ONE SCANNED
    //APPEARS TO GIVE SOME BUGS WHILE INITIATING
    IEnumerator CTA1FadeOut()
    {
        yield return new WaitForSeconds(10);
        CTA1.SetActive(false);
        CTA2.SetActive(true);
        experinceStarted = true;
    }

    void Update()
    {
        //SEE IF WE HAVE PLANES SCANNED
        var screenMiddlePoint = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));

        if (experinceStarted && _arRaycastManager.Raycast(screenMiddlePoint, hits, trackableTypes: TrackableType.PlaneWithinPolygon))
        {
            //DETECTION OF SURFACE AND POSITION AND ROTATION
            var hitPose = hits[0].pose;
            var cameraForward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0 , cameraForward.z).normalized;
            var placementRotation = Quaternion.LookRotation(cameraBearing);

            //PLANES DISAPPEAR LOGIC
            foreach (var plane in _arPlaneManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }
            _arPlaneManager.enabled = false;

            //INDICATOR ACTIVATION LOGIC
            if (hits.Count > 0 && spawnedObject == null)
            {
                indicator.transform.position = hitPose.position;
                indicator.transform.rotation = placementRotation;
                indicator.SetActive(true);
            }
            else
            {
                indicator.SetActive(false);
            }

            //ON TAP TO THE SURFACE FOUND, WE SPAWN THE DUCKY
            if (Input.touchCount > 0 && hits.Count >0)
            {
                if(spawnedObject == null)
                {  
                    spawnedObject = Instantiate(gameObjectToInstantiate, indicator.transform.position, indicator.transform.rotation);
                    ButtonsUI.SetActive(true);
                    CTA2.SetActive(false);
                    CTA1.SetActive(false);
                    setDuckInfo();
                }
            }
        }

        //DUCKY EASTER EGG
        Ray ray = Camera.current.ScreenPointToRay(Input.touches[0].position);
        RaycastHit hit;

        if (Input.GetTouch(0).phase == TouchPhase.Began)
        {
            //DOUBLE TAP CALCULATOR
            var deltaManuel = Time.time - lastTimeTapped ;
            if (deltaManuel < 0.7 && Physics.Raycast(ray, out hit) && spawnedObject)
            { 
                if (hit.transform.tag == "ducky" && EasterEggFound == false)
                {
                    EasterEggFound = true;
                    duckHat.SetActive(true);                     
                }
            }
            lastTimeTapped = Time.time;
        }
    }

    //SET PRIVATE DATA TO SPAWNED DUCKY
    private void setDuckInfo()
    {
        mainDuck = spawnedObject.transform.GetChild(0).gameObject;
        duckBody = mainDuck.transform.GetChild(0).gameObject;
        duckHat =  mainDuck.transform.GetChild(1).gameObject;
        ren = duckBody.GetComponent<MeshRenderer>();
        mat = ren.materials;
        animator = mainDuck.GetComponent<Animator>();
    }

    //CHANGE MATERIAL OF DUCKY
    public void changeMaterialOfObject(string setMaterialName)
    {
        if (setMaterialName == "cyan")
        {
            mat[1] = cyan;
        }
        if (setMaterialName == "magenta")
        {
            mat[1] = magenta;
        }
        if (setMaterialName == "yellow")
        {
            mat[1] = yellow;
        }
        ren.materials = mat;
    }

    //ANIMATION LOGIC FOR DUCKY
    //ANIMATIONS MADE IN UNITY, RATHER DONE IN BLENDER
    public void allAnimationsReset()
    {
        animator.SetBool("animation1", false);
        animator.SetBool("animation2", false);
        animator.SetBool("animation3", false);
    }

    public void playAnimation(string playAnimationIndex)
    {
        if (playAnimationIndex == "1")
        { 
            allAnimationsReset();
            animator.SetBool("animation1", true);
            animator.Play("anim1");
        }
        if (playAnimationIndex == "2")
        {
            allAnimationsReset();
            animator.SetBool("animation2", true);
            animator.Play("anim2");
        }
        if (playAnimationIndex == "3")
        {
            allAnimationsReset();
            animator.SetBool("animation3", true);
            animator.Play("anim3");
        }
    }
}