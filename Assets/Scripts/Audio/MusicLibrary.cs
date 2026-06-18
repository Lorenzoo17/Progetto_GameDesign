using UnityEngine;
public enum MusicID {
    Hub,
    MainMenu,
    SewerDungeon,
    BossFungus,
    BossPipes,
    GameOver,
    Victory
}

[System.Serializable]
public struct MusicTrack {
    public MusicID musicID;
    public AudioClip[] clips;
}

public class MusicLibrary : MonoBehaviour {

    [SerializeField] private MusicTrack[] tracks;

    public AudioClip GetClip(MusicID musicID) {
        foreach (MusicTrack track in tracks) {
            if (track.musicID == musicID) {
                return track.clips[Random.Range(0, track.clips.Length)];
            }
        }

        Debug.LogWarning($"Music track not found: {musicID}");
        return null;
    }
}