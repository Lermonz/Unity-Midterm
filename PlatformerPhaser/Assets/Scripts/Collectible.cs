using UnityEngine;

public class Collectible : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Cheese")) {
            GameBehaviour.Instance.CollectItem(0);
            Destroy(other.gameObject);
        }
    }
}
