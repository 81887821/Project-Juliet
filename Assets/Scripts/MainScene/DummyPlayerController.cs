using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DummyPlayerController : MonoBehaviour {

    public static DummyPlayerController Instance
    {
        get;
        private set;
    }

    public float Speed = 18f;

    public Transform TargetLocation
    {
        get
        {
            return moveTarget;
        }

        set
        {
            moveTarget = value;
            if (movingCoroutine != null)
                StopCoroutine(movingCoroutine);
            movingCoroutine = StartCoroutine(Move(moveTarget));
        }
    }

    private Animator animator;
    private Transform moveTarget;
    private Coroutine movingCoroutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Double instantiation : " + this);
            Destroy(this);
            return;
        }

        Instance = this;
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        StageManager.Instance.GameStateChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        StageManager.Instance.GameStateChanged -= OnGameStateChanged;
    }

    private IEnumerator Move(Transform target)
    {
        float displacement = target.position.x - transform.position.x;

        animator.Play("JuliaWalk");

        if (displacement < 0)
        {
            transform.rotation = new Quaternion(0f, 1f, 0f, 0f);
            while (displacement < 0)
            {
                transform.Translate(-Speed * Time.deltaTime, 0f, 0f, Space.World);
                yield return new WaitForEndOfFrame();
                displacement = target.position.x - transform.position.x;
            }
        }
        else
        {
            transform.rotation = new Quaternion(0f, 0f, 0f, 1f);
            while (displacement > 0)
            {
                transform.Translate(Speed * Time.deltaTime, 0f, 0f, Space.World);
                yield return new WaitForEndOfFrame();
                displacement = target.position.x - transform.position.x;
            }
        }

        animator.Play("JuliaIdle");
        movingCoroutine = null;
    }

    private void OnGameStateChanged(bool gamePaused)
    {
        enabled = !gamePaused;
    }
}
