using UnityEngine;
using System.Collections;
using System;

public class CameraSmoothFollow : MonoBehaviour
{
    public static event Action ActionMovedRoom = delegate { };

    private Vector3 offset;

    [Tooltip("Approximate time it takes to reach the target position.")]
    [Range(0.0f, 2.25f)]
    public float distanceDamp;

    [Tooltip("Approximate time it takes to reach the target rotation.")]
    [Range(0.15f, 0.5f)]
    public float rotationDamp;

    [Tooltip("The maximum unit speed the camera can move per 60 frames.")]
    [Range(0.75f, 1.5f)]
    public float zAxisSpeed;

    [Tooltip("Ratio of transition speed.")]
    public float transitionDelay;

    private Vector3 velocityReference = Vector3.one;
    private Vector3 velocityReferenceRot = Vector3.zero;

    private Transform playerTransform;
    private Room currentRoom;
    private float roomZ, roomX;

    private float[] zClampValues; // [z values]
    private float[] xClampValues; // [z values]

    bool shouldFollowPlayer;

    private void Start()
    {
        offset = new Vector3(0.0f, 16f, -12f);

        zClampValues = new float[2];
        zClampValues[0] = -18f;
        zClampValues[1] = -8f;

        xClampValues = new float[2];
        xClampValues[0] = -1.0f;
        xClampValues[1] = 1.0f;

        shouldFollowPlayer = false;
    }

    private void LateUpdate()
    {
        if (shouldFollowPlayer)
        {
            SmoothFollow();
        }
    }

    private void SmoothFollow()
    {
        if (playerTransform == null) return;

        Vector3 vectorToPlayer = (playerTransform.position + offset) - transform.position;

        vectorToPlayer = new Vector3(vectorToPlayer.x, vectorToPlayer.y, vectorToPlayer.normalized.z * (zAxisSpeed * (Mathf.Abs(vectorToPlayer.z))));

        Vector3 targetPos = transform.position + vectorToPlayer;
        Vector3 positionThisFrame = Vector3.SmoothDamp(transform.position, targetPos, ref velocityReferenceRot, distanceDamp);

        positionThisFrame = ClampPosition(positionThisFrame);

        transform.position = positionThisFrame;
    }

    private Vector3 ClampPosition(Vector3 vecToClamp)
    {
        float x, z;

        // Clamp X value
        //x = Mathf.Clamp(vecToClamp.x, xClampValues[0], xClampValues[1]);
        x = 0.0f + roomX;

        // Clamp Z value
        z = Mathf.Clamp(vecToClamp.z, zClampValues[0] + roomZ, zClampValues[1] + roomZ);

        Vector3 temp = new Vector3(x, vecToClamp.y, z);

        return temp;
    }

    public void ChangeRoom(Room room)
    {
        if (room != currentRoom)
        {
            ActionMovedRoom();

            int transitionCondition = 0;

            // New room is lower
            if (room.transform.position.z < currentRoom.transform.position.z)
            {
                transitionCondition = 1;
            }
            // New room is above
            else if (room.transform.position.z > currentRoom.transform.position.z)
            {
                transitionCondition = 2;
            }

            currentRoom = room;

            roomZ = currentRoom.transform.position.z;
            roomX = currentRoom.transform.position.x;

            // Do Transition
            StopAllCoroutines();
            StartCoroutine(Transition(transitionCondition));
        }
    }

    public void SetRoom(Room room)
    {
        currentRoom = room;
        roomZ = currentRoom.transform.position.z;
        roomX = currentRoom.transform.position.x;
        transform.position = currentRoom.transform.position + offset + new Vector3(0.0f, 15f, -5.0f);
    }

    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }

    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    public void ZoomInToPlayer()
    {
        StartCoroutine(Transition(-1));
    }

    private IEnumerator Transition(int condition)
    {
        shouldFollowPlayer = false;

        float zOffset;

        switch (condition)
        {
            case 1:
                zOffset = zClampValues[1];
                break;
            case 2:
                zOffset = zClampValues[0];
                break;
            default:
                zOffset = offset.z;
                break;
        }

        Vector3 targetPos = new Vector3(currentRoom.transform.position.x + offset.x, 
            currentRoom.transform.position.y + offset.y + playerTransform.position.y, currentRoom.transform.position.z + zOffset);

        while (Vector3.Distance(transform.position, targetPos) > 0.5f)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocityReference, transitionDelay);

            yield return null;
        }

        shouldFollowPlayer = true;
    }

    public float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
