using UnityEngine;

public class GameOver : MonoBehaviour
{
    public void OnPlaySfx(AudioClip clip)
    {
        AudioManager.Instance.PlaySFX(clip);
    }
}
