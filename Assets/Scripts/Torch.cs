using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class Torch : MonoBehaviour
{
    public bool isLit = false;

    [Header("Art Assets")]
    public Sprite unlitSprite;
    public Sprite litSprite;

    [Header("Input Setup")]
    public InputActionReference interactAction; 

    private SpriteRenderer spriteRenderer;
    private Light2D torchLight;
    public bool isPlayerInRange = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        torchLight = GetComponent<Light2D>();

        if (spriteRenderer != null && unlitSprite != null) spriteRenderer.sprite = unlitSprite;
        if (torchLight != null) torchLight.enabled = false;
    }

    void OnEnable()
    {
        if (interactAction != null) interactAction.action.Enable();
    }

    void OnDisable()
    {
        if (interactAction != null) interactAction.action.Disable();
    }

    void Update()
    {
        if (isPlayerInRange && interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            Interact();
        }
    }

    public void Interact()
    {
        if (!isLit)
        {
            isLit = true;

            if (spriteRenderer != null && litSprite != null)
            {
                spriteRenderer.sprite = litSprite;
            }

            if (torchLight != null)
            {
                torchLight.enabled = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = false;
    }
}