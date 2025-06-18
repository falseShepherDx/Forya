using UnityEngine;

public class MusicDontDestrtoy : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
