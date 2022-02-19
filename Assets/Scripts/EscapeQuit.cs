using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EscapeQuit : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Do any pre-quit processing here.

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit(); //No exit code supplied, it's optional
#endif
        }
    }
}
