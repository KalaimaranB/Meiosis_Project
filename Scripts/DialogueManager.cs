using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public List<Dialogue> dialogues;
    public TMP_Text dialogueText;
    public string meiosis2SceneName;
    public string meiosis2EndSceneName;
    private MeiosisManager mm;

    private void Start()
    {
        mm = GetComponent<MeiosisManager>();
    }

    public void StartDialogue(string dialogue)
    {
        StopAllCoroutines();
        StartCoroutine(DialogueProcess(dialogue));
    }

    public void loadNextScene()
    {
        if (mm.Meiosis1 == true)
        {
            SceneManager.LoadSceneAsync(meiosis2SceneName);
        }else if (mm.Meiosis1 == false)
        {
            SceneManager.LoadSceneAsync(meiosis2EndSceneName);
        }
    }

    private IEnumerator DialogueProcess(string dialogue)
    {
        dialogueText.text = "";
        foreach (char letter in dialogue.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    [System.Serializable]
    public class Dialogue
    {
        [TextArea(3,20)] 
        public string sentence;
        
    }
}
