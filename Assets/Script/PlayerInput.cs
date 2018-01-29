using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour
{
    public float HorizontalInput
    {
        get;
        private set;
    }
    public float VerticalInput
    {
        get;
        private set;
    }
    public Vector2 DirectionalInput
    {
        get
        {
            return new Vector2(HorizontalInput, VerticalInput);
        }
    }
    public bool ActionInputDown
    {
        get;
        private set;
    }
    public bool ActionInputUp
    {
        get;
        private set;
    }

    Player player;

	void Start()
	{
		player = GetComponent<Player>();
	}

	void Update()
	{
        HorizontalInput = Input.GetAxisRaw("Horizontal");
        VerticalInput = player.downSideJumpEnabled ? Input.GetAxisRaw("Vertical") : 0.0f;
        ActionInputDown = Input.GetKeyDown(KeyCode.Space);
        ActionInputUp = Input.GetKeyUp(KeyCode.Space);

#if DEBUG
		if (Input.GetKeyDown(KeyCode.Q))
		{
			player.OnPlayerDamaged(10, 1);
		}
		if (Input.GetKeyDown(KeyCode.E))
		{
			player.OnPlayerDamaged(10, -1);
		}
#endif
	}
}
