using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPOV : MonoBehaviour
{
    public float viewRadius = 5f; // How far the player can see
    [Range(0, 360)]
    public float viewAngle = 90f; // Angle of the view cone

    public LayerMask targetMask; // Layers to detect (e.g., enemies)
    public LayerMask obstacleMask; // Layers to block vision (e.g., walls)

    public Transform playerObj; // Reference to the player object (prefab model) to get facing direction
    public Light playerSpotlight; // Reference to the player's main spotlight
    public Light secondSpotlight; // Reference to the second spotlight (to follow player direction)

    private List<Collider> visibleTargets = new List<Collider>(); // List to keep track of visible targets

    void Start()
    {
        //HideAllTargetsAtStart(); // Hide all targets initially
        StartCoroutine(FindTargetsWithDelay(0.2f)); // Start periodic target checks
    }

    private void Update()
    {
        // Make the second spotlight follow the player's direction with a fixed Y position
        if (secondSpotlight != null && playerObj != null)
        {
            // Get the direction the player is facing (forward vector)
            Vector3 playerForward = playerObj.forward;

            // Position the second spotlight behind the player, with a fixed Y-axis
            Vector3 spotlightPosition = playerObj.position - playerForward * 1f; // Adjust the '1f' to change the distance from the player
            spotlightPosition.y = secondSpotlight.transform.position.y; // Keep the spotlight at the same height

            // Set the position of the second spotlight
            secondSpotlight.transform.position = spotlightPosition;

            // Rotate the second spotlight to follow the player's direction (including rotation on Z-axis)
            Quaternion rotation = Quaternion.LookRotation(new Vector3(playerForward.x, 0f, playerForward.z)); // Ignore Y-axis rotation
            secondSpotlight.transform.rotation = rotation; // Apply rotation for spotlight direction
        }
    }

    private void HideAllTargetsAtStart()
    {
        // Hide all targets within the target mask at the start of the game
        Collider[] allTargets = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        foreach (Collider target in allTargets)
        {
            Renderer targetRenderer = target.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                targetRenderer.enabled = false; // Hide all targets
            }
        }
    }

    private IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay); // Delay between checks
            FindVisibleTargets(); // Check for visible targets periodically
        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear(); // Clear the list of visible targets each time we check
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask); // Get targets within view radius

        if (playerObj != null)
        {
            Vector3 forwardDirection = playerObj.forward; // Direction the player is facing

            foreach (Collider target in targetsInViewRadius)
            {
                Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
                float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

                // Check if the target is within the view angle AND within the view radius
                bool isWithinViewAngle = Vector3.Angle(forwardDirection, dirToTarget) < viewAngle / 2;
                bool isWithinViewRadius = distanceToTarget <= viewRadius;

                // Only proceed if the target is within the view angle AND the view radius AND not blocked by obstacles
                if (isWithinViewAngle && isWithinViewRadius && !IsTargetBlockedByObstacle(dirToTarget, target))
                {
                    // Check if the target is within the spotlight range
                    if (IsTargetInLight(target))
                    {
                        visibleTargets.Add(target); // Add target to the visible list if all conditions are met
                        Debug.Log($"{target.name} is visible.");
                    }
                }
            }
        }

        UpdateTargetVisibility();
    }

    bool IsTargetBlockedByObstacle(Vector3 dirToTarget, Collider target)
    {
        // Check if there's an obstacle blocking the line of sight to the target
        float distToTarget = Vector3.Distance(transform.position, target.transform.position);
        return Physics.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask);
    }

    void UpdateTargetVisibility()
    {
        Collider[] allTargets = Physics.OverlapSphere(transform.position, viewRadius, targetMask); // Get all targets in view radius

        // Update visibility of each target
        foreach (Collider target in allTargets)
        {
            Renderer targetRenderer = target.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                // Only show targets that are in the visible targets list
                targetRenderer.enabled = visibleTargets.Contains(target);
            }
        }
    }

    private bool IsTargetInLight(Collider target)
    {
        // Check if the target is within the spotlight's range
        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        return distanceToTarget <= playerSpotlight.range;
    }

    private void OnDrawGizmos()
    {
        // Visualize the view radius and angle in the editor
        if (playerObj == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius); // Draw view radius sphere

        Vector3 forwardDirection = playerObj.forward;
        // Calculate the left and right edges of the view cone
        Vector3 viewAngleA = Quaternion.Euler(0, -viewAngle / 2, 0) * forwardDirection * viewRadius;
        Vector3 viewAngleB = Quaternion.Euler(0, viewAngle / 2, 0) * forwardDirection * viewRadius;

        // Draw the view cone as lines from the player's position
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA); // Left edge of the cone
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB); // Right edge of the cone

        // Draw a shaded cone for better visibility (optional)
        Gizmos.color = new Color(0, 1, 0, 0.1f); // Semi-transparent green for the cone
        Gizmos.DrawMesh(CreateConeMesh(viewRadius, viewAngle / 2), transform.position, Quaternion.LookRotation(forwardDirection));
    }

    // Helper method to create a cone mesh for better visual feedback
    private Mesh CreateConeMesh(float radius, float angle)
    {
        Mesh mesh = new Mesh();
        int segments = 30; // Number of segments for the cone
        float angleStep = angle * 2 / segments;

        Vector3[] vertices = new Vector3[segments + 2];
        vertices[0] = Vector3.zero; // Apex of the cone
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -angle + i * angleStep;
            float x = radius * Mathf.Sin(Mathf.Deg2Rad * currentAngle);
            float z = radius * Mathf.Cos(Mathf.Deg2Rad * currentAngle);
            vertices[i + 1] = new Vector3(x, 0, z);
        }

        int[] triangles = new int[segments * 3];
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % segments + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
}
