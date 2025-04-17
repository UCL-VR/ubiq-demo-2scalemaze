using System;
using System.Collections;
using System.Collections.Generic;
using Ubiq.Rooms;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Box : MonoBehaviour
{
    public int sideNumb = 3;
    public float z_offset = 0.3f;
    public float sideSize =0;
    private int[,] grid;
    private Tile[] tilePositions;
    private bool[] tileRegistered;

    private RoomClient client;
    public NetworkId Id { get; } = new NetworkId("e2ef-ff06-307f-b2ce");
    NetworkContext ctx;

    public static Box rootBox;

    public NetworkId sharedId;

    private bool isBeingGrabbed;

    public struct Message
    {
        public Vector3[] tilePos;
        public NetworkId who;
        public Message(Vector3[] tilePos, NetworkId messenger)
        {
            this.tilePos = tilePos;
            this.who = messenger;
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<Message>();

        // Ignore any messages that we sent.
        if(msg.who == sharedId)
        {
            Debug.Log("Ignoring message from local player");
            return;
        }

        // Process the new tile positions.
        // Create an empty grid.
        for (int i = 0; i < sideNumb * sideNumb; i++)
        {
            grid[Mathf.FloorToInt(i / sideNumb), i % sideNumb] = -1;
        }

        // For each tile 1-8, update its position relative to its box
        for (int i = 0; i < sideNumb * sideNumb-1; i++)
        {
            Tile curTil = tilePositions[i];
            Vector3 curTilPosition = msg.tilePos[i];
            curTil.transform.position = curTil.boxTrans().TransformPoint(curTilPosition);

            Vector3 relativePoint = curTil.transform.position - curTil.boxTrans().position;
            relativePoint = relativePoint / sideSize - new Vector3(-sideNumb / 2, 0, -sideNumb / 2);
            Vector2Int newPosition = new Vector2Int(Mathf.RoundToInt(relativePoint.x) + Mathf.FloorToInt(sideNumb / 2) - 1, Mathf.RoundToInt(relativePoint.z) + Mathf.FloorToInt(sideNumb / 2) - 1);
            grid[newPosition.x, newPosition.y] = curTil.TileId -1;
            curTil.PositionInBox = newPosition;
        }

        // Log some stuff
        //Debug.Log(TileInfo());
    }

    public void Start()
    {
        // Keep a reference to the network
        ctx = NetworkScene.Register(this);
        // Get a reference to the room client
        client = ctx.Scene.GetComponentInChildren<RoomClient>();
        // Register for Peer events
        client.OnPeerAdded.AddListener(OnPeerAdded);
        // Give ourselves a player ID
        sharedId = NetworkId.Unique();
    }

    public void Awake()
    {
        // Keep the box alive
        if (rootBox == null)
        {
            rootBox = this;
            DontDestroyOnLoad(gameObject);
        }
        else // Only one networkscene can exist at the top level of the hierarchy
        {
            gameObject.SetActive(false); // Deactivate the branch to avoid Start() being called until the branch is destroyed
            Destroy(gameObject);
        }

        // Initialize box state
        //sideSize = 16 * transform.lossyScale[0];
        grid = new int[sideNumb, sideNumb];
        //waitingTiles = new List<Tile>[sideNumb, sideNumb];
        tilePositions = new Tile[sideNumb * sideNumb -1];
        tileRegistered = new bool[sideNumb * sideNumb - 1];
        for (int i = 0; i < sideNumb * sideNumb; i++)
        {
            grid[Mathf.FloorToInt(i / sideNumb), i % sideNumb] = -1;
            if(i< sideNumb * sideNumb - 1)
            {
                tilePositions[i] = null;
                tileRegistered[i] = false;
            }
        }
    }

    private void OnPeerAdded(IPeer peer)
    {
        if (peer.uuid == client.Me.uuid)
        {
            return;
        }
        // Send all tile positions
        Debug.Log("Sending tile positions to new player");
        ctx.SendJson(new Message(getRelativeTilePositions(), sharedId));
    }

    public Vector3[] getRelativeTilePositions()
    {
        // Return tile positions relative to the box.
        Vector3[] updatedPos = new Vector3[sideNumb * sideNumb];
        //Debug.Log(TilePosInfo());
        for (int i = 0; i < sideNumb * sideNumb-1; i++)
        {
            updatedPos[i] = tilePositions[i].boxTrans().InverseTransformPoint(tilePositions[i].transform.position);
        }
        return updatedPos;
    }

    public void update_tileOccupation(Tile tile)
    {
        //Since the tile has moved, set the position were the tile was to be empty
        for (int i = 0; i < sideNumb * sideNumb; i++)
        {
            Vector2Int ind = new Vector2Int(Mathf.FloorToInt(i / sideNumb), i % sideNumb);
            if (grid[ind.x, ind.y] == tile.TileId-1)
            {
                grid[ind.x, ind.y] = -1;
            }
        }

        //Set the new tile position
        Vector3 relativePoint = tile.transform.position - tile.boxTrans().position;
        relativePoint = relativePoint / sideSize - new Vector3(-sideNumb / 2, 0, -sideNumb / 2);

        Vector2Int newPosition = new Vector2Int(Mathf.RoundToInt(relativePoint.x) + Mathf.FloorToInt(sideNumb / 2) - 1, Mathf.RoundToInt(relativePoint.z) + Mathf.FloorToInt(sideNumb / 2) - 1);
        grid[newPosition.x, newPosition.y] = tile.TileId-1;
        tile.PositionInBox = newPosition;

        // Log some more stuff.
        Debug.Log(TileInfo());

        //Send final position of tile
        ctx.SendJson(new Message(getRelativeTilePositions(), sharedId));
    }

    public Vector3 getPosFromIndex(Vector2Int index, Transform boxTrans)
    {
        return boxTrans.position + new Vector3((-sideNumb / 2 + index[0]) * sideSize, z_offset, (-sideNumb / 2 + index[1]) * sideSize);
    }

    public Vector2Int declarePosition(Tile tile)
    {
        //Set the size of a side of a mesh depending if the player is inside or outside
        sideSize = 16 * tile.boxTrans().lossyScale[0];

        //Case where a tile has registered with the box manager for the first time
        if (!tileRegistered[tile.TileId - 1])
        {
            tileRegistered[tile.TileId - 1] = true;
            //Register in the box the tile position as default
            tilePositions[tile.TileId - 1] = tile;
            Vector3 relativePoint = tile.transform.position - tile.boxTrans().position;
            relativePoint = relativePoint / sideSize - new Vector3(-sideNumb / 2, 0, -sideNumb / 2);
            Vector2Int newPosition = new Vector2Int(Mathf.RoundToInt(relativePoint.x) + Mathf.FloorToInt(sideNumb / 2) - 1, Mathf.RoundToInt(relativePoint.z) + Mathf.FloorToInt(sideNumb / 2) - 1);
            tile.transform.position = getPosFromIndex(newPosition, tile.boxTrans());
            tile.PositionInBox = newPosition;
            if (grid[newPosition.x, newPosition.y] != -1)
            {
                Debug.Log("WARNING: The position is already used by another tile.");
            }
            grid[newPosition.x, newPosition.y] = tile.TileId - 1;
            return newPosition;
        }
        //Case where the tile is reregistering (when player changes scene)
        tilePositions[tile.TileId - 1] = tile;

        //Set the tile to the one stored at the box manager
        for (int i = 0; i < sideNumb * sideNumb; i++)
        {
            if(grid[Mathf.FloorToInt(i / sideNumb), i % sideNumb] == tile.TileId-1)
            {
                Vector2Int curpos = new Vector2Int(Mathf.FloorToInt(i / sideNumb), i % sideNumb);
                tile.PositionInBox = curpos;
                tile.transform.position = getPosFromIndex(curpos, tile.boxTrans());
                return curpos;
            }
        }
        return new Vector2Int();

    }

    public Vector2Int GetFreeIndex()
    {
        // Returns the index of the empty tile.
        for (int i = 0; i < sideNumb * sideNumb; i++)
        {
            if (grid[Mathf.FloorToInt(i / sideNumb), i % sideNumb] == -1)
            {
                return new Vector2Int(Mathf.FloorToInt(i / sideNumb), i % sideNumb);
            }
        }
        Debug.Log("WARNING: There are no free tile positions");
        return new Vector2Int();
    }

    public bool TryGetMovable(Tile tile, out Vector3 endPointA, out Vector3 endPointB)
    {
        // If a tile is movable, returns the range of free points.
        // Otherwise returns null.

        // Get the index of the free tile.
        Vector2Int free = GetFreeIndex();

        // Calculate the difference in tiles between the free tile and this tile.
        Vector2Int dif = free - tile.PositionInBox;

        // This tile can be moved if it is 1 tile away from the free tile.
        if (Mathf.Abs(dif.x) + Mathf.Abs(dif.y) != 1)
        {
            endPointA = endPointB = Vector3.zero;
            return false;
        }

        // Compute the range of coordinates between our tile and the free tile.
        // Our tile can be moved within this range.
        endPointA = getPosFromIndex(tile.PositionInBox, tile.boxTrans());
        endPointB = getPosFromIndex(free, tile.boxTrans());
        return true;
    }

    string TileInfo() {
        // Return a string for debugging the state of each tile.
        string msgOut = "Synching: grid structure ";
        for (int i = 0; i < sideNumb * sideNumb; i++)
        {
            if (grid[Mathf.FloorToInt(i / sideNumb), i % sideNumb] != -1)
            {
                msgOut += (Mathf.FloorToInt(i / sideNumb), i % sideNumb) + "->" + grid[Mathf.FloorToInt(i / sideNumb), i % sideNumb] + " ";
            }
            else
            {
                msgOut += (Mathf.FloorToInt(i / sideNumb), i % sideNumb) + "->null";
            }
        }
        return msgOut;
    }

    string TilePosInfo()
    {
        // Return a string for debugging the state of each tile.
        string msgOut = "Synching ";
        for (int i = 0; i < sideNumb * sideNumb-1; i++)
        {

            msgOut += "Tile " + i + "->" + tilePositions[i] + " ";

        }
        return msgOut;
    }

    // Allow tiles to inform us when they are being grabbed
    public void TileGrabbed() {
        isBeingGrabbed = true;
    }

    public void TileReleased() {
        isBeingGrabbed = false;
    }

    // Update is called once per frame
    void Update()
    {
        // While a tile is being grabbed, let the party know about it.
        if (isBeingGrabbed)
        {
            ctx.SendJson(new Message(getRelativeTilePositions(), sharedId));
        }
    }


}
