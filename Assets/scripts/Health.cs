using UnityEngine;

public class Health : MonoBehaviour
{
    private int _healthCount;
    public int HealthCount => _healthCount;

    public int maxHealth = 5;
    void Start()
    {
        _healthCount = maxHealth;
    }

    public void LoseHealth(int amountLost)
    {
       _healthCount -= amountLost;
       if(_healthCount <= 0)
        {
            Destroy(gameObject);
        } 
    }
}
