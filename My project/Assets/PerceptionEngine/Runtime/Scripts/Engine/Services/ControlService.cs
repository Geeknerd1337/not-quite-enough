using UnityEngine;


namespace Perception.Engine
{
    public class ControlService : PerceptionService
    {
        public bool CursorLocked { get; set; }


        public override void Awake()
        {
            base.Awake();
            CursorLocked = true;


        }

        public override void Update()
        {
            base.Update();
            if (!CursorLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

    }
}
