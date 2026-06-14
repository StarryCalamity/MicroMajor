using UnityEngine;

// A single collectible coin. It sits in its idle animation until the player
// touches it, at which point Collect() plays the one-shot pickup burst and the
// coin removes itself once the burst has finished.
public class Coin : MonoBehaviour
{
    // How many points this coin is worth.
    public int value = 1;

    // Should roughly match the length of the coin_pickup clip so the coin stays
    // visible for the whole burst before it is destroyed.
    public float pickupAnimationLength = 0.45f;

    private bool collected = false;
    private Animator animator;
    private Collider2D coinCollider;

    void Awake()
    {
        animator = GetComponent<Animator>();
        coinCollider = GetComponent<Collider2D>();
    }

    // Collects the coin. Returns the value gained, or 0 if it was already
    // collected (so it can never be counted twice).
    public int Collect()
    {
        if (collected)
        {
            return 0;
        }
        collected = true;

        // Stop further trigger hits while the burst plays out.
        if (coinCollider != null)
        {
            coinCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.SetTrigger("Collected");
        }

        Destroy(gameObject, pickupAnimationLength);
        return value;
    }
}
