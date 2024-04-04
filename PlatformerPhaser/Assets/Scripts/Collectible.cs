using UnityEngine;

public class Collectible : MonoBehaviour
{
    public static bool itemGot = false;
    GameObject allCheeses;
    [SerializeField] AudioClip collectionClip;
    void Start() {
        allCheeses = GameObject.Find("Collectibles");
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Cheese")) {
            GameBehaviour.Instance.CollectItem();
            ItemCollected();
            GetComponent<AudioSource>().PlayOneShot(collectionClip);
        }
    }
    public void ItemCollected() {
        itemGot = true;
        allCheeses.transform.position = new Vector3(-50,0,0);
    }
    public void ResetPosition() {
        itemGot = false;
        allCheeses.transform.position = Vector3.zero;
    }
    void Update() {
        if((PlayerMovement._isDead || PlayerMovement._isLevelTransition) && itemGot) {
            ResetPosition();
        }
    }
}
