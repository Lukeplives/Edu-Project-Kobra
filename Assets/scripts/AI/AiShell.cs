using UnityEngine;

public class AiShell : MonoBehaviour
{
    public GameObject explosion;
    Rigidbody body;
    [SerializeField] int damage;

    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.tag == "Player")
        {
            //GameObject exp = Instantiate(explosion,transform.position, Quaternion.identity);
            //Destroy(exp, 0.5f);
            Health playerHealth = collider.gameObject.GetComponent<Health>();
            playerHealth.LoseHealth(damage);
            Destroy(gameObject);
        }
    }

}
