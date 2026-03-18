using UnityEngine;

/// <summary>
/// Trigger-based barrier at the school entrance in Khortitskiy District.
/// Blocks the player from entering unless they have the "Bakhily" (shoe covers) item.
/// When blocked, Zavkhoz plays a "stop" animation (placeholder).
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class BakhilyBarrier : MonoBehaviour
{
    [Header("Barrier Settings")]
    [Tooltip("Tag used to identify the player character.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Name of the Zavkhoz GameObject in the scene.")]
    [SerializeField] private string zavkhozObjectName = "Zavkhoz";

    [Tooltip("Name of the stop animation trigger parameter.")]
    [SerializeField] private string stopAnimTrigger = "Stop";

    [Header("Barrier Force")]
    [Tooltip("Force applied to push the player back when blocked.")]
    [SerializeField] private float pushBackForce = 8f;

    [Tooltip("How far back from the entrance to push the player.")]
    [SerializeField] private float pushBackDistance = 2f;

    [Header("UI")]
    [Tooltip("Message displayed when the player is blocked.")]
    [SerializeField] private string blockedMessage = "Без бахіл не пущу! (No shoe covers — no entry!)";

    [Tooltip("Duration the blocked message stays on screen.")]
    [SerializeField] private float messageDisplayTime = 3f;

    private GameObject zavkhozObject;
    private Animator zavkhozAnimator;
    private bool isShowingMessage;
    private float messageTimer;
    private GUIStyle messageStyle;

    private void Start()
    {
        // Ensure the collider is a trigger
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }

        // Find the Zavkhoz character
        zavkhozObject = GameObject.Find(zavkhozObjectName);
        if (zavkhozObject != null)
        {
            zavkhozAnimator = zavkhozObject.GetComponent<Animator>();
        }

        // Set up GUI style for the blocked message
        messageStyle = new GUIStyle
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };
        messageStyle.normal.textColor = Color.white;
    }

    private void Update()
    {
        if (isShowingMessage)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f)
            {
                isShowingMessage = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        // Check if the player has "Bakhily" item
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        bool hasBakhily = inventory != null && inventory.HasItem("Bakhily");

        if (hasBakhily)
        {
            // Player has shoe covers — allow entry
            Debug.Log("[BakhilyBarrier] Player has bakhily. Entry allowed.");
            return;
        }

        // Block the player
        Debug.Log("[BakhilyBarrier] Player does NOT have bakhily. Entry BLOCKED.");
        BlockPlayer(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        bool hasBakhily = inventory != null && inventory.HasItem("Bakhily");

        if (!hasBakhily)
        {
            // Continue blocking
            PushPlayerBack(other);
        }
    }

    private void BlockPlayer(Collider playerCollider)
    {
        // Show blocked message
        isShowingMessage = true;
        messageTimer = messageDisplayTime;

        // Play Zavkhoz's stop animation
        PlayZavkhozStopAnimation();

        // Push the player back
        PushPlayerBack(playerCollider);
    }

    private void PushPlayerBack(Collider playerCollider)
    {
        // Try using Rigidbody for physics-based push
        Rigidbody playerRb = playerCollider.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            Vector3 pushDirection = (playerCollider.transform.position - transform.position).normalized;
            pushDirection.y = 0f;
            playerRb.AddForce(pushDirection * pushBackForce, ForceMode.Impulse);
        }
        else
        {
            // Fallback: teleport the player back
            CharacterController characterController = playerCollider.GetComponent<CharacterController>();
            Vector3 pushDirection = (playerCollider.transform.position - transform.position).normalized;
            pushDirection.y = 0f;

            if (characterController != null)
            {
                characterController.Move(pushDirection * pushBackDistance);
            }
            else
            {
                playerCollider.transform.position += pushDirection * pushBackDistance;
            }
        }
    }

    private void PlayZavkhozStopAnimation()
    {
        if (zavkhozAnimator != null)
        {
            zavkhozAnimator.SetTrigger(stopAnimTrigger);
            Debug.Log("[BakhilyBarrier] Zavkhoz plays stop animation.");
        }
        else if (zavkhozObject != null)
        {
            // Placeholder: rotate Zavkhoz to face the player and raise arm
            StartCoroutine(PlaceholderStopAnimation());
        }
    }

    private System.Collections.IEnumerator PlaceholderStopAnimation()
    {
        if (zavkhozObject == null) yield break;

        // Find arm to animate (placeholder)
        Transform arm = zavkhozObject.transform.Find("Zavkhoz_Arm_0");
        if (arm != null)
        {
            // Raise arm
            Quaternion startRot = arm.localRotation;
            Quaternion targetRot = Quaternion.Euler(-90f, 0f, 0f);

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                arm.localRotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }

            // Hold position
            yield return new WaitForSeconds(1.5f);

            // Lower arm
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                arm.localRotation = Quaternion.Slerp(targetRot, startRot, t);
                yield return null;
            }
        }
    }

    private void OnGUI()
    {
        if (!isShowingMessage) return;

        // Semi-transparent background
        float boxWidth = 600f;
        float boxHeight = 80f;
        float boxX = (Screen.width - boxWidth) * 0.5f;
        float boxY = Screen.height - boxHeight - 50f;

        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(new Rect(boxX, boxY, boxWidth, boxHeight), Texture2D.whiteTexture);

        GUI.color = Color.white;
        GUI.Label(new Rect(boxX, boxY, boxWidth, boxHeight), blockedMessage, messageStyle);
    }
}

/// <summary>
/// Simple inventory component for the player.
/// Tracks items the player has collected.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private System.Collections.Generic.List<string> items = new System.Collections.Generic.List<string>();

    /// <summary>
    /// Check if the player has a specific item.
    /// </summary>
    public bool HasItem(string itemName)
    {
        return items.Contains(itemName);
    }

    /// <summary>
    /// Add an item to the inventory.
    /// </summary>
    public void AddItem(string itemName)
    {
        if (!items.Contains(itemName))
        {
            items.Add(itemName);
            Debug.Log("[PlayerInventory] Acquired: " + itemName);
        }
    }

    /// <summary>
    /// Remove an item from the inventory.
    /// </summary>
    public void RemoveItem(string itemName)
    {
        if (items.Remove(itemName))
        {
            Debug.Log("[PlayerInventory] Removed: " + itemName);
        }
    }
}
