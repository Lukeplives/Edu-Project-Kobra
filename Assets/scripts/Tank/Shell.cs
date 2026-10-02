using UnityEngine;

public class Shell : MonoBehaviour {

    public GameObject explosion;
    [SerializeField] int damage;

    void OnTriggerEnter(Collider collider) {

        if (collider.gameObject.tag == "Enemy") {
            // GameObject exp = Instantiate(explosion, this.transform.position, Quaternion.identity);
            // Destroy(exp, 0.5f);
            Health enemyHealth = collider.gameObject.GetComponentInParent<Health>();
            enemyHealth.LoseHealth(damage);
            Destroy(this.gameObject);
        }
    }

}
