using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Trigger-based barrier at the school entrance in Khortitskiy District.
/// Blocks the player from entering unless they have the "Bakhily" (shoe covers) item.
/// When blocked, Zavkhoz plays a "stop" animation (placeholder).
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class BakhilyBarrier : MonoBehaviour
{
    [Header("Barrier Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string zavkhozObjectName = "Zavkhoz";
    [SerializeField] private string stopAnimTrigger = "Stop";

    [Header("Barrier Force")]
    [SerializeField] private float pushBackForce = 8f;
    [SerializeField] private float pushBackDistance = 2f;

    [Header("UI")]
    [SerializeField] private string blockedMessage = "\u0411\u0435\u0437 \u0431\u0430\u0445\u0456\u043b \u043d\u0435 \u043f\u0443\u0449\u0443! (No shoe covers \u2014 no entry!)";
    [SerializeField] private float messageDisplayTime = 3f;

    private GameObject zavkhozObject;
    private Animator zavkhozAnimator;
    private bool isShowingMessage;
    private float messageTimer;
    private GUIStyle messageStyle;

    private void Start()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }

        zavkhozObject = GameObject.Find(zavkhozObjectName);
        if (zavkhozObject != null)
        {
            zavkhozAnimator = zavkhozObject.GetComponent<Animator>();
        }

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
        if (!other.CompareTag(playerTag)) return;

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        bool hasBakhily = inventory != null && inventory.HasItem("Bakhily");

        if (hasBakhily)
        {
            Debug.Log("[BakhilyBarrier] Player has bakhily. Entry allowed.");
            return;
        }

        Debug.Log("[BakhilyBarrier] Player does NOT have bakhily. Entry BLOCKED.");
        BlockPlayer(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        bool hasBakhily = inventory != null && inventory.HasItem("Bakhily");

        if (!hasBakhily)
        {
            PushPlayerBack(other);
        }
    }

    private void BlockPlayer(Collider playerCollider)
    {
        isShowingMessage = true;
        messageTimer = messageDisplayTime;
        PlayZavkhozStopAnimation();
        PushPlayerBack(playerCollider);
    }

    private void PushPlayerBack(Collider playerCollider)
    {
        Rigidbody playerRb = playerCollider.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            Vector3 pushDirection = (playerCollider.transform.position - transform.position).normalized;
            pushDirection.y = 0f;
            playerRb.AddForce(pushDirection * pushBackForce, ForceMode.Impulse);
        }
        else
        {
            CharacterController cc = playerCollider.GetComponent<CharacterController>();
            Vector3 pushDirection = (playerCollider.transform.position - transform.position).normalized;
            pushDirection.y = 0f;

            if (cc != null)
                cc.Move(pushDirection * pushBackDistance);
            else
                playerCollider.transform.position += pushDirection * pushBackDistance;
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
            StartCoroutine(PlaceholderStopAnimation());
        }
    }

    private IEnumerator PlaceholderStopAnimation()
    {
        if (zavkhozObject == null) yield break;

        Transform arm = zavkhozObject.transform.Find("Zavkhoz_Arm_0");
        if (arm != null)
        {
            Quaternion startRot = arm.localRotation;
            Quaternion targetRot = Quaternion.Euler(-90f, 0f, 0f);
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                arm.localRotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
                yield return null;
            }

            yield return new WaitForSeconds(1.5f);

            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                arm.localRotation = Quaternion.Slerp(targetRot, startRot, elapsed / duration);
                yield return null;
            }
        }
    }

    private void OnGUI()
    {
        if (!isShowingMessage) return;

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
