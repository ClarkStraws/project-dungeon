using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;
using System;

public class ObjectGeneration : MonoBehaviour
{
    Tilemap tilemap;

    [Header("Object Prefabs")]

    [SerializeField]
    public GameObject[] loreObjects;

    public GameObject[] safeObjects;

    public GameObject[] dangerObjects;

    public GameObject[] unassignedObjects;

    Dictionary<Vector3Int, bool> placedObjects = new Dictionary<Vector3Int, bool>();

    // This is how we're managing the count of unique instances of objects that have variants
    public ObjectCountManager objectCountManager;

    public ObjectCounts objectCounter = new ObjectCounts();

    public void Awake()
    {
        tilemap = GameObject.Find("Main Tilemap").GetComponent<Tilemap>();
    }

    public void GenerateObjectPlacements(List<GameObject> rooms)
    {
        foreach (GameObject roomObj in rooms)
        {

            Room room = roomObj.GetComponent<Room>();


            switch (room.roomType)
            {

                case Enums.RoomType.Safe:

                    PopulateRoom(safeObjects, room);

                    break;

                case Enums.RoomType.Danger:
                    PopulateRoom(dangerObjects, room);

                    break;

                case Enums.RoomType.Lore:

                    PopulateRoom(loreObjects, room);

                    break;

                default:
                    break;

            }

        }

    }


    public void PopulateRoom(GameObject[] objects, Room room)
    {

        // Only get subtype if we are dealing with lore rooms for now.
        // TODO: make this more scalable

        List<GameObject> roomObjects = new List<GameObject>();

        if (room.roomType == Enums.RoomType.Lore)
        {
            Enums.RoomSubType subType = GetRandomRoomSubType();
            roomObjects = objects.Where(x => x.GetComponent<ObjectBehavior>().RoomSubTypes.Contains(subType)).ToList();
        }

        else
        {
            roomObjects = objects.ToList();
        }

        // Randomly sort the list before using it so that we don't use the same objects in the same order every time


        // TODO:
        // Go keep track of maxCount outside each individual room object. Otherwise we just start over every time and get the same count each time

        roomObjects.Shuffle();

        foreach (GameObject roomObject in roomObjects)
        {
            StartCoroutine(DoPlacementCheck(roomObject, room));
        }


    }


    IEnumerator DoPlacementCheck(GameObject roomObject, Room room)
    {
        ObjectBehavior roomObjectBehavior = roomObject.GetComponent<ObjectBehavior>();

        PlacementRule placementRule = GetPlacementRuleByObject(roomObjectBehavior);

        Enums.ObjectType objectType = roomObjectBehavior.ObjectType;

        int attempt = 0;
        int maxAllowed = objectCountManager.GetCountAllowedByObjectType(objectType);
        // max is some amount between 1 and the max allowed of the object type
        int max = GetRandomNumberOfObjects(maxAllowed);

        int numCreated = objectCounter.GetCountByType(objectType);

        while (attempt < 100 && numCreated < max)
        {

            Vector3Int position = placementRule.GetPointInRoom(room);

            if (placementRule.CanPlaceObject(tilemap, position, roomObjectBehavior.Width, roomObjectBehavior.Height))
            {

                GameObject testObject = Instantiate(roomObject, position, Quaternion.identity);

                Collider2D collider = testObject.transform.GetChild(0).GetComponent<Collider2D>();

                LayerMask mask = 1 << LayerMask.NameToLayer("ObjectPlacementLayer");

                yield return new WaitForFixedUpdate();

                if (collider.IsTouchingLayers(mask))
                {
                    Destroy(testObject);
                }

                else
                {
                    testObject.transform.parent = room.gameObject.transform.GetChild(1).transform;
                    ++numCreated;
                    objectCounter.SetCountByType(objectType, numCreated);
                }


            }

            attempt++;

        }
    }

    private PlacementRule GetPlacementRuleByObject(ObjectBehavior roomObject)
    {

        switch (roomObject.PlacementType)
        {

            case Enums.PlacementType.Floor:
                return new FloorPlacementRule();

            case Enums.PlacementType.UpperWall:
                return new UpperWallPlacementRule();

            case Enums.PlacementType.SideWall:
                return new SideWallPlacementRule();

            default:
                return new FloorPlacementRule();

        }
    }

    private int GetRandomNumberOfObjects(int maxAllowed)
    {

        return UnityEngine.Random.Range(1, maxAllowed + 1);
    }

    private Enums.RoomSubType GetRandomRoomSubType()
    {

        int rand = UnityEngine.Random.Range(0, Enum.GetNames(typeof(Enums.RoomSubType)).Length);

        Enums.RoomSubType loreRoomType = (Enums.RoomSubType)rand;

        return loreRoomType;
    }


    public class ObjectCounts
    {
        // Running totals by object type
        public int bedCount = 0;
        public int candleCount = 0;
        public int chestCount = 0;
        public int debrisCount = 0;
        public int bookShelfCount = 0;

        public void SetCountByType(Enums.ObjectType objectType, int count)
        {
            if (objectType == Enums.ObjectType.Bed) { bedCount = count; }
            else if (objectType == Enums.ObjectType.Bookshelf) { bookShelfCount = count; }
            else if (objectType == Enums.ObjectType.Candle) { candleCount = count; }
            else if (objectType == Enums.ObjectType.Chest) { chestCount = count; }
            else if (objectType == Enums.ObjectType.Debris) { debrisCount = count; }
            else
            {
                return;
            }
        }


        public int GetCountByType(Enums.ObjectType objectType)
        {
            if (objectType == Enums.ObjectType.Bed) { return bedCount; }
            else if (objectType == Enums.ObjectType.Bookshelf) { return bookShelfCount; }
            else if (objectType == Enums.ObjectType.Candle) { return candleCount; }
            else if (objectType == Enums.ObjectType.Chest) { return chestCount; }
            else if (objectType == Enums.ObjectType.Debris) { return debrisCount; }
            else
            {
                return 1000; // huge number if we don't actually have a maximum defined
            }
        }

    }

}