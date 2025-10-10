using UnityEngine;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform door; // The rotating part of the door
    public float openAngle = 100f;
    public float openSpeed = 2f;
    private bool isOpen = false;

    [Header("Audio")]
    public AudioSource knockAudio;
    public AudioSource doorOpenAudio;

    [Header("NPC Settings")]
    public GameObject[] guests; // List of NPCs who will appear
    public Transform guestSpawnPoint;
    private int currentGuestIndex = 0;

    [Header("Player Interaction")]
    public Transform playerCamera;
    public float interactDistance = 2f;
    public KeyCode interactKey = KeyCode.E; // For testing in editor
    private bool canInteract = false;

    [Header("Guest Timing")]
    public float nextGuestDelay = 5f; // Time between guests knocking
    public float knockDelay = 3f; // Time before knocking starts

    private Quaternion initialRotation;
    private Quaternion openRotation;
    private bool guestWaiting = false;

    void Start()
    {
        initialRotation = door.localRotation;
        openRotation = Quaternion.Euler(door.localEulerAngles + new Vector3(0, openAngle, 0));
        StartCoroutine(GuestKnockSequence());
    }

    void Update()
    {
        // Distance check for interaction
        if (playerCamera != null)
        {
            float dist = Vector3.Distance(playerCamera.position, transform.position);
            canInteract = dist <= interactDistance;
        }

        // Player opens door
        if (guestWaiting && canInteract && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(OpenDoor());
        }
    }

    IEnumerator GuestKnockSequence()
    {
        yield return new WaitForSeconds(knockDelay);

        if (currentGuestIndex < guests.Length)
        {
            // Play knocking sound
            if (knockAudio != null)
                knockAudio.Play();

            guestWaiting = true;
        }
    }

    IEnumerator OpenDoor()
    {
        guestWaiting = false;
        isOpen = true;

        if (doorOpenAudio != null)
            doorOpenAudio.Play();

        // Smooth open animation
        float t = 0;
        Quaternion startRot = door.localRotation;
        while (t < 1)
        {
            t += Time.deltaTime * openSpeed;
            door.localRotation = Quaternion.Slerp(startRot, openRotation, t);
            yield return null;
        }

        // Spawn NPC
        yield return new WaitForSeconds(0.3f);
        SpawnGuest();

        // Wait for conversation or manual trigger
        yield return new WaitForSeconds(5f); // Simulate conversation time

        // Close door again
        StartCoroutine(CloseDoor());
    }

    IEnumerator CloseDoor()
    {
        isOpen = false;

        float t = 0;
        Quaternion startRot = door.localRotation;
        while (t < 1)
        {
            t += Time.deltaTime * openSpeed;
            door.localRotation = Quaternion.Slerp(startRot, initialRotation, t);
            yield return null;
        }

        // Hide guest and prep next
        if (currentGuestIndex < guests.Length)
            guests[currentGuestIndex].SetActive(false);

        currentGuestIndex++;
        yield return new WaitForSeconds(nextGuestDelay);

        // Trigger next guest
        if (currentGuestIndex < guests.Length)
            StartCoroutine(GuestKnockSequence());
    }

    void SpawnGuest()
    {
        if (currentGuestIndex < guests.Length && guestSpawnPoint != null)
        {
            GameObject guest = guests[currentGuestIndex];
            guest.transform.position = guestSpawnPoint.position;
            guest.SetActive(true);

            // Here you could start dialogue or animation
            Debug.Log("Guest " + guest.name + " is ready to talk.");
        }
    }
}
