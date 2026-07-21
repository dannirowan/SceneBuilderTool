using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LiftButton : MonoBehaviour
{
    public LiftController controller;
    public int targetFloorIndex;
    [Tooltip("Enable legacy mouse-click interaction. Keep disabled for FPS reticle + E interaction.")]
    public bool allowMouseClickInteraction = false;

    [SerializeField] private float pressCooldown = 0.15f;
    private float lastPressTime = -1f;

    // Direct click support (standard Unity collider detection)
    private void OnMouseDown()
    {
        if (!allowMouseClickInteraction)
        {
            return;
        }

        PressButton();
    }

    // Call this if using standard FPS Raycasting or Interaction toolkits
    public void PressButton()
    {
        if (Time.unscaledTime - lastPressTime < pressCooldown)
        {
            return;
        }

        lastPressTime = Time.unscaledTime;

        if (controller != null)
        {
            controller.MoveToFloor(targetFloorIndex);
        }
        else
        {
            Debug.LogWarning($"LiftButton '{name}' has no LiftController assigned.");
        }
    }
}