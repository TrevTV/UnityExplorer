/*using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using MelonLoader;
using UniverseLib.Input;

namespace UnityExplorer
{
    [MelonLoader.RegisterTypeInIl2Cpp]
    public class LaserPointerInputModule : MonoBehaviour {

        public LaserPointerInputModule(System.IntPtr ptr) : base(ptr) { }

        #region BaseInputModule
        [NonSerialized]
        protected List<RaycastResult> m_RaycastResultCache = new List<RaycastResult>();

        private AxisEventData m_AxisEventData;

        private EventSystem m_EventSystem;
        private BaseEventData m_BaseEventData;

        protected BaseInput m_InputOverride;
        private BaseInput m_DefaultInput;

        /// <summary>
        /// The current BaseInput being used by the input module.
        /// </summary>
        public BaseInput input
        {
            get
            {
                if (m_InputOverride != null)
                    return m_InputOverride;

                if (m_DefaultInput == null)
                {
                    var inputs = GetComponents<BaseInput>();
                    foreach (var baseInput in inputs)
                    {
                        // We dont want to use any classes that derrive from BaseInput for default.
                        if (baseInput != null && baseInput.GetType() == typeof(BaseInput))
                        {
                            m_DefaultInput = baseInput;
                            break;
                        }
                    }

                    if (m_DefaultInput == null)
                        m_DefaultInput = gameObject.AddComponent<BaseInput>();
                }

                return m_DefaultInput;
            }
        }

        /// <summary>
        /// Used to override the default BaseInput for the input module.
        /// </summary>
        /// <remarks>
        /// With this it is possible to bypass the Input system with your own but still use the same InputModule. For example this can be used to feed fake input into the UI or interface with a different input system.
        /// </remarks>
        public BaseInput inputOverride
        {
            get { return m_InputOverride; }
            set { m_InputOverride = value; }
        }

        protected EventSystem eventSystem
        {
            get { return m_EventSystem; }
        }

        protected void OnEnable()
        {
            //base.OnEnable();
            m_EventSystem = GetComponent<EventSystem>();
            m_EventSystem.UpdateModules();
        }

        protected void OnDisable()
        {
            m_EventSystem.UpdateModules();
            //base.OnDisable();
        }

        /// <summary>
        /// Return the first valid RaycastResult.
        /// </summary>
        protected static RaycastResult FindFirstRaycast(List<RaycastResult> candidates)
        {
            var candidatesCount = candidates.Count;
            for (var i = 0; i < candidatesCount; ++i)
            {
                if (candidates[i].gameObject == null)
                    continue;

                return candidates[i];
            }
            return new RaycastResult();
        }

        /// <summary>
        /// Given an input movement, determine the best MoveDirection.
        /// </summary>
        /// <param name="x">X movement.</param>
        /// <param name="y">Y movement.</param>
        protected static MoveDirection DetermineMoveDirection(float x, float y)
        {
            return DetermineMoveDirection(x, y, 0.6f);
        }

        /// <summary>
        /// Given an input movement, determine the best MoveDirection.
        /// </summary>
        /// <param name="x">X movement.</param>
        /// <param name="y">Y movement.</param>
        /// <param name="deadZone">Dead zone.</param>
        protected static MoveDirection DetermineMoveDirection(float x, float y, float deadZone)
        {
            // if vector is too small... just return
            if (new Vector2(x, y).sqrMagnitude < deadZone * deadZone)
                return MoveDirection.None;

            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                return x > 0 ? MoveDirection.Right : MoveDirection.Left;
            }

            return y > 0 ? MoveDirection.Up : MoveDirection.Down;
        }

        /// <summary>
        /// Given 2 GameObjects, return a common root GameObject (or null).
        /// </summary>
        /// <param name="g1">GameObject to compare</param>
        /// <param name="g2">GameObject to compare</param>
        /// <returns></returns>
        protected static GameObject FindCommonRoot(GameObject g1, GameObject g2)
        {
            if (g1 == null || g2 == null)
                return null;

            var t1 = g1.transform;
            while (t1 != null)
            {
                var t2 = g2.transform;
                while (t2 != null)
                {
                    if (t1 == t2)
                        return t1.gameObject;
                    t2 = t2.parent;
                }
                t1 = t1.parent;
            }
            return null;
        }

        // walk up the tree till a common root between the last entered and the current entered is found
        // send exit events up to (including) the common root. Then send enter events up to
        // (including) the common root.
        // Send move events before exit, after enter, and on hovered objects when pointer data has changed.
        protected void HandlePointerExitAndEnter(PointerEventData currentPointerData, GameObject newEnterTarget)
        {
            // if we have no target / pointerEnter has been deleted
            // just send exit events to anything we are tracking
            // then exit
            if (newEnterTarget == null || currentPointerData.pointerEnter == null)
            {
                var hoveredCount = currentPointerData.hovered.Count;
                for (var i = 0; i < hoveredCount; ++i)
                {
                    //currentPointerData.fullyExited = true;
                    //ExecuteEvents.Execute(currentPointerData.hovered[i], currentPointerData, ExecuteEvents.pointerMoveHandler);
                    ExecuteEvents.Execute(currentPointerData.hovered[i], currentPointerData, ExecuteEvents.pointerExitHandler);
                }

                currentPointerData.hovered.Clear();

                if (newEnterTarget == null)
                {
                    currentPointerData.pointerEnter = null;
                    return;
                }
            }

            // if we have not changed hover target
            if (currentPointerData.pointerEnter == newEnterTarget && newEnterTarget)
            {
                if (currentPointerData.IsPointerMoving())
                {
                    var hoveredCount = currentPointerData.hovered.Count;
                    //for (var i = 0; i < hoveredCount; ++i)
                    //    ExecuteEvents.Execute(currentPointerData.hovered[i], currentPointerData, ExecuteEvents.pointerMoveHandler);
                }
                return;
            }

            GameObject commonRoot = FindCommonRoot(currentPointerData.pointerEnter, newEnterTarget);

            // and we already an entered object from last time
            if (currentPointerData.pointerEnter != null)
            {
                // send exit handler call to all elements in the chain
                // until we reach the new target, or null!
                Transform t = currentPointerData.pointerEnter.transform;

                while (t != null)
                {
                    if (t.gameObject == newEnterTarget && t.gameObject == commonRoot)
                    {
                        break;
                    }

                    //currentPointerData.fullyExited = t.gameObject != commonRoot && currentPointerData.pointerEnter != newEnterTarget;
                    //ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerMoveHandler);
                    ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerExitHandler);
                    currentPointerData.hovered.Remove(t.gameObject);

                    // if we reach the common root break out!
                    if (commonRoot != null && commonRoot.transform == t)
                        break;
                    t = t.parent;
                }
            }

            // now issue the enter call up to but not including the common root
            currentPointerData.pointerEnter = newEnterTarget;
            if (newEnterTarget != null)
            {
                Transform t = newEnterTarget.transform;

                while (t != null)
                {
                    if (t.gameObject != newEnterTarget && t.gameObject == commonRoot)
                    {
                        break;
                    }
                    //currentPointerData.reentered = t.gameObject == commonRoot;
                    ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerEnterHandler);
                    //ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerMoveHandler);
                    currentPointerData.hovered.Add(t.gameObject);

                    if (commonRoot != null && commonRoot.transform == t)
                        break;
                    t = t.parent;
                }
            }
        }

        /// <summary>
        /// Given some input data generate an AxisEventData that can be used by the event system.
        /// </summary>
        /// <param name="x">X movement.</param>
        /// <param name="y">Y movement.</param>
        /// <param name="deadZone">Dead zone.</param>
        protected virtual AxisEventData GetAxisEventData(float x, float y, float moveDeadZone)
        {
            if (m_AxisEventData == null)
                m_AxisEventData = new AxisEventData(eventSystem);

            m_AxisEventData.Reset();
            m_AxisEventData.moveVector = new Vector2(x, y);
            m_AxisEventData.moveDir = DetermineMoveDirection(x, y, moveDeadZone);
            return m_AxisEventData;
        }

        /// <summary>
        /// Generate a BaseEventData that can be used by the EventSystem.
        /// </summary>
        protected virtual BaseEventData GetBaseEventData()
        {
            if (m_BaseEventData == null)
                m_BaseEventData = new BaseEventData(eventSystem);

            m_BaseEventData.Reset();
            return m_BaseEventData;
        }
        #endregion

        public static LaserPointerInputModule instance { get { return _instance; } }
        private static LaserPointerInputModule _instance = null;

        public LayerMask layerMask;

        // storage class for controller specific data
        private class ControllerData {
            public LaserPointerEventData pointerEvent;
            public GameObject currentPoint;
            public GameObject currentPressed;
            public GameObject currentDragging;
        };

        public static bool UseMe(Type __result)
        {
            MelonLogger.Msg("forcing me!");
            __result = typeof(LaserPointerInputModule);
            return false;
        }

        private Camera UICamera;
        private PhysicsRaycaster raycaster;
        private HashSet<IUILaserPointer> _controllers;
        // controller data
        private Dictionary<IUILaserPointer, ControllerData> _controllerData = new Dictionary<IUILaserPointer, ControllerData>();

        protected void Awake()
        {
            //base.Awake();

            if(_instance != null) {
                Debug.LogWarning("Trying to instantiate multiple LaserPointerInputModule.");
                DestroyImmediate(this.gameObject);
            }

            _instance = this;
        }

        protected void Start()
        {
            // Create a new camera that will be used for raycasts
            UICamera = new GameObject("UI Camera").AddComponent<Camera>();
            // Added PhysicsRaycaster so that pointer events are sent to 3d objects
            raycaster = UICamera.gameObject.AddComponent<PhysicsRaycaster>();
            UICamera.clearFlags = CameraClearFlags.Nothing;
            UICamera.enabled = false;
            UICamera.fieldOfView = 5;
            UICamera.nearClipPlane = 0.01f;

            // Find canvases in the scene and assign our custom
            // UICamera to them
            Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();            
            foreach(Canvas canvas in canvases) {
                canvas.worldCamera = UICamera;
            }
        }

        public void AddController(IUILaserPointer controller)
        {
            _controllerData.Add(controller, new ControllerData());
        }

        public void RemoveController(IUILaserPointer controller)
        {
            _controllerData.Remove(controller);
        }

        protected void UpdateCameraPosition(IUILaserPointer controller)
        {
            UICamera.transform.position = controller.transform.position;
            UICamera.transform.rotation = controller.transform.rotation;
        }

        // clear the current selection
        public void ClearSelection()
        {
            if(eventSystem.currentSelectedGameObject) {
                eventSystem.SetSelectedGameObject(null);
            }
        }

        // select a game object
        private void Select(GameObject go)
        {
            ClearSelection();

            if(ExecuteEvents.GetEventHandler<ISelectHandler>(go)) {
                eventSystem.SetSelectedGameObject(go);
            }
        }

        public void Process()
        {
            raycaster.eventMask = layerMask;

            *//*foreach (var pair in_controllerData)
            {
                IUILaserPointer controller = pair.Key;
                ControllerData data = pair.Value;

                // Test if UICamera is looking at a GUI element
                UpdateCameraPosition(controller);

                if (data.pointerEvent == null)
                    data.pointerEvent = new LaserPointerEventData(t.eventSystem);
                else
                    data.pointerEvent.Reset();

                data.pointerEvent.controller = controller;
                data.pointerEvent.delta = Vector2.zero;
                data.pointerEvent.position = new Vector2(t.UICamera.pixelWidth * 0.5f, UICamera.pixelHeight * 0.5f);
                //data.pointerEvent.scrollDelta = Vector2.zero;

                // trigger a raycast
                eventSystem.RaycastAll(data.pointerEvent, m_RaycastResultCache);
                data.pointerEvent.pointerCurrentRaycast = FindFirstRaycast(t.m_RaycastResultCache);
                m_RaycastResultCache.Clear();

                // make sure our controller knows about the raycast result
                // we add 0.01 because that is the near plane distance of our camera and we want the correct distance
                if (data.pointerEvent.pointerCurrentRaycast.distance > 0.0f)
                    controller.LimitLaserDistance(data.pointerEvent.pointerCurrentRaycast.distance + 0.01f);

                // stop if no UI element was hit
                //if(pointerEvent.pointerCurrentRaycast.gameObject == null)
                //return;

                // Send control enter and exit events to our controller
                var hitControl = data.pointerEvent.pointerCurrentRaycast.gameObject;
                if (data.currentPoint != hitControl)
                {
                    if (data.currentPoint != null)
                        controller.OnExitControl(data.currentPoint);

                    if (hitControl != null)
                        controller.OnEnterControl(hitControl);
                }

                data.currentPoint = hitControl;

                // Handle enter and exit events on the GUI controlls that are hit
                HandlePointerExitAndEnter(data.pointerEvent, data.currentPoint);

                if (controller.ButtonDown())
                {
                    ClearSelection();

                    data.pointerEvent.pressPosition = data.pointerEvent.position;
                    data.pointerEvent.pointerPressRaycast = data.pointerEvent.pointerCurrentRaycast;
                    data.pointerEvent.pointerPress = null;

                    // update current pressed if the curser is over an element
                    if (data.currentPoint != null)
                    {
                        data.currentPressed = data.currentPoint;
                        data.pointerEvent.current = data.currentPressed;
                        GameObject newPressed = ExecuteEvents.ExecuteHierarchy(data.currentPressed, data.pointerEvent, ExecuteEvents.pointerDownHandler);
                        ExecuteEvents.Execute(controller.gameObject, data.pointerEvent, ExecuteEvents.pointerDownHandler);
                        if (newPressed == null)
                        {
                            // some UI elements might only have click handler and not pointer down handler
                            newPressed = ExecuteEvents.ExecuteHierarchy(data.currentPressed, data.pointerEvent, ExecuteEvents.pointerClickHandler);
                            ExecuteEvents.Execute(controller.gameObject, data.pointerEvent, ExecuteEvents.pointerClickHandler);
                            if (newPressed != null)
                            {
                                data.currentPressed = newPressed;
                            }
                        }
                        else
                        {
                            data.currentPressed = newPressed;
                            // we want to do click on button down at same time, unlike regular mouse processing
                            // which does click when mouse goes up over same object it went down on
                            // reason to do this is head tracking might be jittery and this makes it easier to click buttons
                            ExecuteEvents.Execute(newPressed, data.pointerEvent, ExecuteEvents.pointerClickHandler);
                            ExecuteEvents.Execute(controller.gameObject, data.pointerEvent, ExecuteEvents.pointerClickHandler);

                        }

                        if (newPressed != null)
                        {
                            data.pointerEvent.pointerPress = newPressed;
                            data.currentPressed = newPressed;
                            Select(data.currentPressed);
                        }

                        ExecuteEvents.Execute(data.currentPressed, data.pointerEvent, ExecuteEvents.beginDragHandler);
                        ExecuteEvents.Execute(controller.gameObject, data.pointerEvent, ExecuteEvents.beginDragHandler);

                        data.pointerEvent.pointerDrag = data.currentPressed;
                        data.currentDragging = data.currentPressed;
                    }
                }// button down end


                if (controller.ButtonUp())
                {
                    if (data.currentDragging != null)
                    {
                        data.pointerEvent.current = data.currentDragging;
                        ExecuteEvents.Execute(data.currentDragging, data.pointerEvent, ExecuteEvents.endDragHandler);
                        ExecuteEvents.Execute(controller.gameObject, data.pointerEvent, ExecuteEvents.endDragHandler);
                        if (data.currentPoint != null)
                        {
                            ExecuteEvents.ExecuteHierarchy(data.currentPoint, data.pointerEvent, ExecuteEvents.dropHandler);
                        }
                        data.pointerEvent.pointerDrag = null;
                        data.currentDragging = null;
                    }
                    if (data.currentPressed)
                    {
                        data.pointerEvent.current = data.currentPressed;
                        ExecuteEvents.Execute(data.currentPressed, data.pointerEvent, ExecuteEvents.pointerUpHandler);
                        ExecuteEvents.Execute(controller.gameObject, data.pointerEvent, ExecuteEvents.pointerUpHandler);
                        data.pointerEvent.rawPointerPress = null;
                        data.pointerEvent.pointerPress = null;
                        data.currentPressed = null;
                    }
                }




                // drag handling
                if (data.currentDragging != null)
                {
                    data.pointerEvent.current = data.currentPressed;
                    ExecuteEvents.Execute(data.currentDragging, data.pointerEvent, ExecuteEvents.dragHandler);
                    ExecuteEvents.Execute(controller.gameObject, data.pointerEvent, ExecuteEvents.dragHandler);
                }



                // update selected element for keyboard focus
                if (t.eventSystem.currentSelectedGameObject != null)
                {
                    data.pointerEvent.current = eventSystem.currentSelectedGameObject;
                    ExecuteEvents.Execute(t.eventSystem.currentSelectedGameObject, GetBaseEventData(), ExecuteEvents.updateSelectedHandler);
                    //ExecuteEvents.Execute(controller.gameObject, GetBaseEventData(), ExecuteEvents.updateSelectedHandler);
                }
            }*//*
        }
    }
}*/