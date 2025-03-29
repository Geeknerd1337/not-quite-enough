using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Perception.Engine;
using Unity.VisualScripting;

public class IntroLevel : MonoBehaviour
{

    public SoundObject CarAudio;

    public Transform Camera;

    public AnimationCurve FadeCurve;

    public DialogData IntroDialog;

    // Start is called before the first frame update
    void Start()
    {
        var a = PerceptionAudio.FromScreen(CarAudio);
        a.volume = 0f;
        PerceptionAudio.FadeTo(a, 5f, 1f);

        var player = GameManager.Pawn as Player;
        var controller = player.GetComponent<PlayerController>();
        controller.DisableController();

        FadeCanvas.FadeTo(Color.black.WithAlpha(1f), Color.black.WithAlpha(0f), 5f, FadeCurve);

        StartCoroutine(StartDialog());

    }

    public IEnumerator StartDialog()
    {
        yield return new WaitForSeconds(5f);
        DialogManager.StartDialog(IntroDialog);
    }

    // Update is called once per frame
    void Update()
    {
        var player = GameManager.Pawn as Player;
        var tripod = player.gameObject.GetComponent<FirstpersonTripod>();
        tripod.SetTargetOverride(Camera);
    }
}
