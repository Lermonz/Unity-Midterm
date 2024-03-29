using UnityEngine;

public class Powerup : MonoBehaviour
{
    public static bool itemGot = false;
    GameObject allPowerUps;
    void Start() {
        allPowerUps = GameObject.Find("Powerups");
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Item")) {
            ItemCollected();
        }
    }
    public void ItemCollected() {
        itemGot = true;
        allPowerUps.transform.position = new Vector3(-50,0,0);
    }
    public void ResetPosition() {
        itemGot = false;
        allPowerUps.transform.position = Vector3.zero;
    }
    void Update() {
        if(PlayerMovement._isDead || PlayerMovement._isLevelTransition) {
            ResetPosition();
        }
    }
}
