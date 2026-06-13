using System.Collections.Generic;
using UnityEngine;

public class GeneratorScript : MonoBehaviour
{
    // Room prefabs that can be spawned. For this game there is just one, but the
    // array makes it easy to add more varied rooms later.
    public GameObject[] availableRooms;

    // Rooms that currently exist in the scene.
    public List<GameObject> currentRooms;

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
