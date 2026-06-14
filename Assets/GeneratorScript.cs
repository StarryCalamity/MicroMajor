using System.Collections.Generic;
using UnityEngine;

public class GeneratorScript : MonoBehaviour
{
    // Room prefabs that can be spawned. For this game there is just one, but the
    // array makes it easy to add more varied rooms later.
    public GameObject[] availableRooms;

    // Rooms that currently exist in the scene.
    public List<GameObject> currentRooms;

    // Coin prefab scattered into each newly spawned room.
    public GameObject coinPrefab;

    // Per-slot chance (0..1) that a coin formation is dropped while walking across
    // the room. High value = rooms stay mostly full with only short gaps.
    public float coinSpawnChance = 0.85f;

    // Spacing between neighbouring coins in a formation (world units).
    public float coinSpacing = 0.7f;

    // Vertical band the formation's base point is placed in.
    public float coinMinY = -1.0f;
    public float coinMaxY = 1.5f;

    // A few reusable coin formation templates. Each is a list of (x, y) offsets
    // measured in coinSpacing units from the formation's base point.
    private static readonly Vector2[][] CoinTemplates =
    {
        // straight line
        new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(2, 0), new Vector2(3, 0), new Vector2(4, 0) },
        // arch
        new[] { new Vector2(0, 0), new Vector2(1, 0.8f), new Vector2(2, 1.1f), new Vector2(3, 0.8f), new Vector2(4, 0) },
        // valley / V
        new[] { new Vector2(0, 1.1f), new Vector2(1, 0.5f), new Vector2(2, 0), new Vector2(3, 0.5f), new Vector2(4, 1.1f) },
        // zigzag
        new[] { new Vector2(0, 0), new Vector2(1, 0.9f), new Vector2(2, 0), new Vector2(3, 0.9f), new Vector2(4, 0) },
        // diamond
        new[] { new Vector2(1, 1), new Vector2(0, 0), new Vector2(2, 0), new Vector2(1, -1), new Vector2(1, 0) },
        // vertical column
        new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 2) },
    };

    // Width of the screen in world units, used to decide when to add/remove rooms.
    private float screenWidthInPoints;

    void Start()
    {
        float height = 2.0f * Camera.main.orthographicSize;
        screenWidthInPoints = height * Camera.main.aspect;
    }

    void Update()
    {
        GenerateRoomIfRequired();
    }

    // Instantiates a random room right after the farthest existing room.
    void AddRoom(float farthestRoomEndX)
    {
        int randomRoomIndex = Random.Range(0, availableRooms.Length);

        GameObject room = (GameObject)Instantiate(availableRooms[randomRoomIndex]);

        float roomWidth = room.transform.Find("floor").localScale.x;
        float roomCenter = farthestRoomEndX + roomWidth * 0.5f;

        room.transform.position = new Vector3(roomCenter, 0, 0);

        currentRooms.Add(room);

        AddCoins(room, roomCenter, roomWidth);
    }

    // Walks across the room dropping random coin formations back-to-back so there
    // are no long empty stretches. Coins are parented to the room so they are
    // cleaned up together with it.
    void AddCoins(GameObject room, float roomCenter, float roomWidth)
    {
        if (coinPrefab == null)
        {
            return;
        }

        float halfWidth = roomWidth * 0.5f;
        float left = roomCenter - halfWidth + 1f;
        float right = roomCenter + halfWidth - 1f;

        float x = left;
        while (x < right)
        {
            if (Random.value > coinSpawnChance)
            {
                // Occasional empty slot so it is not perfectly wall-to-wall.
                x += 2f * coinSpacing;
                continue;
            }

            Vector2[] template = CoinTemplates[Random.Range(0, CoinTemplates.Length)];
            float baseY = Random.Range(coinMinY, coinMaxY);

            float maxOffsetX = 0f;
            foreach (Vector2 offset in template)
            {
                float px = x + offset.x * coinSpacing;
                // Clamp so no template can poke into the floor or ceiling.
                float py = Mathf.Clamp(baseY + offset.y * coinSpacing, -2.4f, 3.0f);
                GameObject coin = Instantiate(coinPrefab, new Vector3(px, py, 0), Quaternion.identity);
                coin.transform.parent = room.transform;

                if (offset.x > maxOffsetX)
                {
                    maxOffsetX = offset.x;
                }
            }

            // Advance past this formation plus a one-coin gap.
            x += (maxOffsetX + 2f) * coinSpacing;
        }
    }

    void GenerateRoomIfRequired()
    {
        // Rooms that have scrolled off-screen and should be destroyed.
        List<GameObject> roomsToRemove = new List<GameObject>();

        bool addRooms = true;

        float playerX = transform.position.x;

        // A room is removed once it is one full screen behind the player and a new
        // room is added once there is less than one screen of rooms ahead.
        float removeRoomX = playerX - screenWidthInPoints;
        float addRoomX = playerX + screenWidthInPoints;

        float farthestRoomEndX = 0;

        foreach (var room in currentRooms)
        {
            float roomWidth = room.transform.Find("floor").localScale.x;
            float roomStartX = room.transform.position.x - (roomWidth * 0.5f);
            float roomEndX = roomStartX + roomWidth;

            // There is already a room ahead of the player, no need to add one.
            if (roomStartX > addRoomX)
            {
                addRooms = false;
            }

            // This room is fully behind the player, queue it for removal.
            if (roomEndX < removeRoomX)
            {
                roomsToRemove.Add(room);
            }

            farthestRoomEndX = Mathf.Max(farthestRoomEndX, roomEndX);
        }

        foreach (var room in roomsToRemove)
        {
            currentRooms.Remove(room);
            Destroy(room);
        }

        if (addRooms)
        {
            AddRoom(farthestRoomEndX);
        }
    }
}
