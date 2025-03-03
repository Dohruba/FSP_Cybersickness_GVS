using UnityEngine;

public class CoinController : MonoBehaviour
{
    private int value = 1;
    [SerializeField]
    private float speed = 1;

    private void FixedUpdate()
    { 
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        LevelsManager.CollectCoin();
        LevelsManager.DespawnMe(gameObject);
    }
}
