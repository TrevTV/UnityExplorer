﻿using UnityEngine;

namespace UnityExplorer
{
    [MelonLoader.RegisterTypeInIl2Cpp]
    public class IUILaserPointer : MonoBehaviour
    {
        public IUILaserPointer(System.IntPtr ptr) : base(ptr) { }

        public float laserThickness = 0.002f;
        public float laserHitScale = 0.02f;
        public bool laserAlwaysOn = false;
        public Color color = Color.blue;

        private GameObject hitPoint;
        private GameObject pointer;


        private float _distanceLimit;

        // Use this for initialization
        void Start()
        {
            // todo:    let the user choose a mesh for laser pointer ray and hit point
            //          or maybe abstract the whole menu control some more and make the 
            //          laser pointer a module.
            pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pointer.transform.SetParent(transform, false);
            pointer.transform.localScale = new Vector3(laserThickness, laserThickness, 100.0f);
            pointer.transform.localPosition = new Vector3(0.0f, 0.0f, 50.0f);

            hitPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hitPoint.transform.SetParent(transform, false);
            hitPoint.transform.localScale = new Vector3(laserHitScale, laserHitScale, laserHitScale);
            hitPoint.transform.localPosition = new Vector3(0.0f, 0.0f, 100.0f);

            hitPoint.SetActive(false);

            // remove the colliders on our primitives
            GameObject.DestroyImmediate(hitPoint.GetComponent<SphereCollider>());
            GameObject.DestroyImmediate(pointer.GetComponent<BoxCollider>());
            
            Material newMaterial = new Material(Shader.Find("Wacki/LaserPointer"));

            newMaterial.SetColor("_Color", color);
            pointer.GetComponent<MeshRenderer>().material = newMaterial;
            hitPoint.GetComponent<MeshRenderer>().material = newMaterial;
            // initialize concrete class
            Initialize();
            
            // register with the LaserPointerInputModule
            if(LaserPointerInputModule.instance == null) {
                new GameObject().AddComponent<LaserPointerInputModule>();
            }
            

            LaserPointerInputModule.instance.AddController(this);
        }

        void OnDestroy()
        {
            if(LaserPointerInputModule.instance != null)
                LaserPointerInputModule.instance.RemoveController(this);
        }

        protected void Initialize() { }
        public void OnEnterControl(GameObject control) { }
        public void OnExitControl(GameObject control) { }
        public bool ButtonDown()
        {
            if (!BoneLib.Player.controllersExist)
                return false;
            return BoneLib.Player.rightController.GetAButtonDown();
        }
        public bool ButtonUp()
        {
            if (!BoneLib.Player.controllersExist)
                return false;
            return BoneLib.Player.rightController.GetAButtonUp();
        }

        protected void Update()
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hitInfo;
            bool bHit = Physics.Raycast(ray, out hitInfo);

            float distance = 100.0f;

            if(bHit) {
                distance = hitInfo.distance;
            }

            // ugly, but has to do for now
            if(_distanceLimit > 0.0f) {
                distance = Mathf.Min(distance, _distanceLimit);
                bHit = true;
            }

            pointer.transform.localScale = new Vector3(laserThickness, laserThickness, distance);
            pointer.transform.localPosition = new Vector3(0.0f, 0.0f, distance * 0.5f);

            if(bHit) {
                hitPoint.SetActive(true);
                hitPoint.transform.localPosition = new Vector3(0.0f, 0.0f, distance);
            }
            else {
                hitPoint.SetActive(false);
            }

            // reset the previous distance limit
            _distanceLimit = -1.0f;
        }

        // limits the laser distance for the current frame
        public void LimitLaserDistance(float distance)
        {
            if(distance < 0.0f)
                return;

            if(_distanceLimit < 0.0f)
                _distanceLimit = distance;
            else
                _distanceLimit = Mathf.Min(_distanceLimit, distance);
        }
    }

}