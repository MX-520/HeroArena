using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay = 2f;

    public void RespawnPlayer(PlayerHealth playerHealth)
    {
        StartCoroutine(RespawnCoroutine(playerHealth));
    }

    private IEnumerator RespawnCoroutine(PlayerHealth playerHealth)
    {
        yield return new WaitForSeconds(respawnDelay);

        playerHealth.Respawn(respawnPoint.position);
    }
}