using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{
    public float distance = 25f;

    public bool continuiousSpawn = true;
    public GameObject enemy;
    public Transform playerTransform;

    private bool playerInArea = false;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(EnemySpawn());
    }

    // Update is called once per frame
    void Update()
    {
        playerInArea = (Vector3.Distance(playerTransform.position, this.transform.position) < distance);
    }

    IEnumerator EnemySpawn()
    {
        if (continuiousSpawn)
        {
            if(playerInArea)
            {
                enemy.GetComponent<PlayerNavMesh>().movePositionTransform = playerTransform;
                enemy.GetComponent<CharacterAnimator>().playerTransform = playerTransform;
                enemy.GetComponent<LookAtPlayer>().lookAt = playerTransform.Find("PlayerHead");
                Instantiate(enemy, this.transform.position, Quaternion.identity);
            }
        
            yield return new WaitForSeconds(5);
            StartCoroutine(EnemySpawn());
        }
            
    }



}
