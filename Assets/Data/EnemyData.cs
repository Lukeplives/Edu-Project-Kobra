using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int Heatlh = 5;
    public Material enemyMaterial;

}
