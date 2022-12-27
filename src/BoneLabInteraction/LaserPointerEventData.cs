/*using MelonLoader;
using UnityEngine.EventSystems;
using static MelonLoader.MelonLogger;

namespace UnityExplorer
{
    public class LaserPointerEventData
    {
        protected bool m_Used;

        public GameObject current;
        public IUILaserPointer controller;
        private readonly EventSystem m_EventSystem;

        public LaserPointerEventData(EventSystem eventSystem)
        {
            m_EventSystem = eventSystem;
        }

        /// <summary>
        /// >A reference to the BaseInputModule that sent this event.
        /// </summary>
        public BaseInputModule currentInputModule
        {
            get { return m_EventSystem.currentInputModule; }
        }

        /// <summary>
        /// The object currently considered selected by the EventSystem.
        /// </summary>
        public GameObject selectedObject
        {
            get { return m_EventSystem.currentSelectedGameObject; }
            set { m_EventSystem.SetSelectedGameObject(value, this); }
        }

        public void Reset()
        {
            m_Used = false;
            current = null;
            controller = null;
        }

        /// <summary>
        /// Use the event.
        /// </summary>
        /// <remarks>
        /// Internally sets a flag that can be checked via used to see if further processing should happen.
        /// </remarks>
        public void Use()
        {
            m_Used = true;
        }

        /// <summary>
        /// Is the event used?
        /// </summary>
        public bool used
        {
            get { return m_Used; }
        }
    }
}

*/