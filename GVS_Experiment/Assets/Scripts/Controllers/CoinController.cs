using UnityEngine;

public class CoinController : MonoBehaviour
{
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
