using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Perception.Engine;

public class IntroLevel : MonoBehaviour
{

    public SoundObject CarAudio;


    // Start is called before the first frame update
    void Start()
    {
        var a = PerceptionAudio.FromScreen(CarAudio);
        a.volume = 0f;
        PerceptionAudio.FadeTo(a, 5f, 1f);

        var player = GameManager.Pawn as Player;
        var controller = player.GetComponent<PlayerController>();
        controller.DisableController();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
