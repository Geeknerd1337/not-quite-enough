using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Perception.Engine
{
    public class FirstpersonTripod : Tripod
    {
        public Transform TargetOverride;


        public override void OnTripodBuild(ref Setup setup)
        {
            var ply = GameManager.Pawn as Player;

            if (ply != null && TargetOverride == null)
            {
                MouseRotateEyes(ply);
                setup.Position = ply.Eyes.position;
                setup.Rotation = ply.Eyes.rotation;
            }


            if (TargetOverride != null)
            {
                MouseRotateEyes(ply);
                setup.Position = TargetOverride.position;
                setup.Rotation = ply.Eyes.rotation;
            }
        }

        public void MouseRotateEyes(Player p)
        {
            //Use the eyes helper to rotate it in accordance with the input
            float x = Input.GetAxisRaw("Mouse X") * 2f;
            float y = Input.GetAxisRaw("Mouse Y") * 2f;
            p.EyesHelper.Rotate(x, y);

            //Rotate the whole object on the z-axis
            transform.localRotation *= Quaternion.Euler(0, p.EyesHelper.rotX, 0);

            //Rotate the eyes up and down with the camera
            var yQuat = Quaternion.AngleAxis(p.EyesHelper.rotY, Vector3.left);
            p.Eyes.localRotation = yQuat;

        }

        public void SetTargetOverride(Transform target)
        {
            TargetOverride = target;
        }

        public void ClearTargetOverride()
        {
            TargetOverride = null;
        }


    }
}
