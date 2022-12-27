using SLZ.Rig;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityExplorer.UI;
using static Il2CppSystem.Globalization.CultureInfo;

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

        // Input Module Dragging/Hover Implementations
        private GameObject currentDragging;
        private GameObject currentPoint;
        private BaseEventData eventData;

        void Start()
        {
            eventData = new BaseEventData(UniverseLib.Input.EventSystemHelper.CurrentEventSystem);
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
            
            Material newMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit (PBR Workflow)"));

            newMaterial.SetColor("_BaseColor", color);
            pointer.GetComponent<MeshRenderer>().material = newMaterial;
            hitPoint.GetComponent<MeshRenderer>().material = newMaterial;
        }

        protected void Update()
        {
            if (!BoneLib.Player.controllersExist) return;

            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hitInfo;
            int uiMask = 1 << LayerMask.NameToLayer("UI");
            bool bHit = Physics.Raycast(ray, out hitInfo, 100, uiMask, QueryTriggerInteraction.Collide);

            float distance = 100.0f;

            if (bHit)
            {
                distance = hitInfo.distance;
            }

            // ugly, but has to do for now
            if (_distanceLimit > 0.0f)
            {
                distance = Mathf.Min(distance, _distanceLimit);
                bHit = true;
            }

            pointer.transform.localScale = new Vector3(laserThickness, laserThickness, distance);
            pointer.transform.localPosition = new Vector3(0.0f, 0.0f, distance * 0.5f);

            bool buttonDown = BoneLib.Player.rightController.GetPrimaryInteractionButtonDown();
            bool buttonUp = BoneLib.Player.rightController.GetPrimaryInteractionButtonUp();
            if (bHit && hitInfo.collider.isTrigger && InHierarchyOf(hitInfo.collider.transform, "UniverseLibCanvas"))
            {
                hitPoint.SetActive(true);
                hitPoint.transform.localPosition = new Vector3(0.0f, 0.0f, distance);
                Selectable b = hitInfo.collider.GetComponent<Selectable>();
                currentPoint = b.gameObject;
                if (b != null)
                {
                    if (buttonDown)
                    {
                        ExecuteEvents.Execute(currentPoint, eventData, ExecuteEvents.submitHandler);
                        ExecuteEvents.Execute(currentPoint, eventData, ExecuteEvents.beginDragHandler);
                        currentDragging = currentPoint;
                    }
                }

                if (buttonUp)
                {
                    if (currentDragging != null)
                    {
                        ExecuteEvents.Execute(currentDragging, eventData, ExecuteEvents.endDragHandler);
                        ExecuteEvents.Execute(currentPoint, eventData, ExecuteEvents.dropHandler);
                        currentDragging = null;
                    }
                }
            }
            else
            {
                hitPoint.SetActive(false);
            }

            // reset the previous distance limit
            _distanceLimit = -1.0f;

            if (currentDragging != null)
            {
                //data.pointerEvent.current = data.currentPressed;
                ExecuteEvents.Execute(currentDragging, eventData, ExecuteEvents.dragHandler);
                //ExecuteEvents.Execute(controller.gameObject, data.pointerEvent, ExecuteEvents.dragHandler);
            }
        }

        public static bool InHierarchyOf(Transform t, string parentName)
        {
            if (t.name == parentName)
                return true;

            if (t.parent == null)
                return false;

            t = t.parent;

            return InHierarchyOf(t, parentName);
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