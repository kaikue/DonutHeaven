using UnityEngine;

public class Lookahead : MonoBehaviour
{
    public Rigidbody2D playerRB;
    public int lookaheadAmount;
    private float maxLookahead;

    private Vector2 delta;
    private Vector2 lastPlayerPos;

	private void Start()
	{

        maxLookahead = lookaheadAmount * (Player.maxRunSpeed + 1) * Time.fixedDeltaTime;
    }

	private void FixedUpdate()
    {
        Vector2 playerDelta = playerRB.position - lastPlayerPos; //use instead of playerRB.velocity.x in case player is running into a wall
        float targetDX = Mathf.Sign(playerDelta.x) * Mathf.Min(lookaheadAmount * Mathf.Abs(playerDelta.x), maxLookahead);
        float dx = Mathf.Lerp(delta.x, targetDX, 0.1f);
        delta = new Vector2(dx, 0);
        transform.position = playerRB.position + delta;
        lastPlayerPos = playerRB.position;
    }
}
