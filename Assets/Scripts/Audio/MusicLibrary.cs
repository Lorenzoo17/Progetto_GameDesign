using UnityEngine;
public enum MusicID {
    MainMenu,
    SewerDungeon,
    BossFungus,
    GameOver,
    Victory
}

[System.Serializable]
public struct MusicTrack {
    public MusicID musicID;
    public AudioClip clip;
}

public class MusicLibrary : MonoBehaviour {

    [SerializeField] private MusicTrack[] tracks;

    public AudioClip GetClip(MusicID musicID) {
        foreach (MusicTrack track in tracks) {
            if (track.musicID == musicID) {
                return track.clip;
            }
        }

        Debug.LogWarning($"Music track not found: {musicID}");
        return null;
    }
}