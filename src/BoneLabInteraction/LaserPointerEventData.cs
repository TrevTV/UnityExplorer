using MelonLoader;
using UnityEngine.EventSystems;

namespace UnityExplorer
{
    public class LaserPointerEventData : PointerEventData
    {
        public GameObject current;
        public IUILaserPointer controller;
        public LaserPointerEventData(EventSystem e) : base(e) { }

        public static void Reset2(PointerEventData __instance)
        {
            MelonLogger.Msg("event : " + __instance.GetActualType().Name);
            MelonLogger.Msg("event2 : " + __instance.GetType().Name);
            if (__instance.GetActualType() == typeof(LaserPointerEventData))
            {
                LaserPointerEventData t = (LaserPointerEventData) __instance;
                t.current = null;
                t.controller = null;
            }
        }
    }
}

