using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
public class StageEntry : MonoBehaviour
{
    public string SceneName;
    public Sprite SpriteOnStageAvailable;
    public Sprite SpriteOnStageUnavailable;
    public bool AlwaysAvailable = false;

    private SpriteRenderer spriteRenderer;
    private bool visited;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        StageManager.Instance.GameStateChanged += OnGameStateChanged;
        visited = StageManager.Visited(SceneName);

        if (AlwaysAvailable || visited)
            spriteRenderer.sprite = SpriteOnStageAvailable;
        else
            spriteRenderer.sprite = SpriteOnStageUnavailable;
    }

    private void OnDestroy()
    {
        StageManager.Instance.GameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(bool gamePaused)
    {
        enabled = !gamePaused;
    }

    private void OnMouseDown()
    {
        if (enabled)
        {
            var player = DummyPlayerController.Instance;

            if (player.TargetLocation != transform)
                player.TargetLocation = transform;
            else if (AlwaysAvailable || visited)
                SceneManager.LoadScene("Scenes/" + SceneName);
        }
    }
}
