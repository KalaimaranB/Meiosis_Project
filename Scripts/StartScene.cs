using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    public string meiosis1SceneName;
    public void Start()
    {
        if (FindObjectOfType<AudioSource>())
        {
            Destroy(FindObjectOfType<AudioSource>().gameObject);
        }
    }
    public void LoadMeiosis1()
    {
        SceneManager.LoadSceneAsync(meiosis1SceneName);
    }
}
