using Unity.Netcode;
using UnityEngine;

public class ZombieSpawner : NetworkBehaviour
{
    [SerializeField] GameObject zombiePrefab;
    [SerializeField] float spawnInterval = 5f;
    [SerializeField] float minDistance = 10f;
    [SerializeField] float maxDistance = 15f;
    [SerializeField] int maxZombies = 20;

    float timer;
    int alive;

    void Update()
    {
        if (!IsServer) return;

        timer += Time.deltaTime;
        if (timer < spawnInterval || alive >= maxZombies) return;
        timer = 0;

        var players = NetworkManager.Singleton.ConnectedClientsList;
        if (players.Count == 0) return;

        var player = players[Random.Range(0, players.Count)].PlayerObject;
        if (player == null) return;

        Vector2 dir = Random.insideUnitCircle.normalized;
        float dist = Random.Range(minDistance, maxDistance);
        Vector3 pos = player.transform.position + new Vector3(dir.x * dist, dir.y * dist, 0f);

        var zombie = Instantiate(zombiePrefab, pos, Quaternion.identity);
        zombie.GetComponent<NetworkObject>().Spawn();
        alive++;
    }

    public void OnZombieDied() => alive--;
}