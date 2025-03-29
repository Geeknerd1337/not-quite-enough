using UnityEngine;

[CreateAssetMenu(fileName = "New Dialog", menuName = "Dialog/Dialog Data")]
public class DialogData : ScriptableObject
{
    [System.Serializable]
    public class DialogLine
    {
        public string speakerName;

        [TextArea(3, 10)]
        public string dialogText;


    }

    public DialogLine[] dialogLines;
}