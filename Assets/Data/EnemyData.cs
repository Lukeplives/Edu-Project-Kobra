using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int Heatlh = 5;
    public float moveSpeed = 3f;
    public float fireRate = 1f;
    public Material enemyMaterial;

}
