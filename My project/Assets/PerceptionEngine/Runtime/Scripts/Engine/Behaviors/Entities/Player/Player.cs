using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Perception.Engine
{
    public class Player : Actor
    {
        public override void Start()
        {
            base.Start();
            gameObject.AddComponent<FirstpersonTripod>();
            Controller = gameObject.GetComponent<PlayerController>();
        }

        public override void BuildInput()
        {
            base.BuildInput();
            Controller.BuildInput();
        }


    }
}
