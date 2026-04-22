using UnityEngine;

public class AnimationAutoDestroy : MonoBehaviour {
    // Distruzione del gameObject alla fine dell'animazione corrente partita e dopo il delay specificato
    public float delay = 0f;

    void Start() {
        Destroy(gameObject, this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + delay);
    }
}
