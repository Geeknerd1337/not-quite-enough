using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;
    public GameObject DialogBoxPrefab;

    void Awake()
    {
        Instance = this;
    }

    public static void StartDialog(DialogData dialogData)
    {
        GameObject dialogBox = Instantiate(Instance.DialogBoxPrefab, Instance.transform);
        DialogBox dialogBoxComponent = dialogBox.GetComponent<DialogBox>();
        dialogBoxComponent.DialogData = dialogData;
        dialogBoxComponent.StartDialog();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
