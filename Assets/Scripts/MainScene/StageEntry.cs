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

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        
    }

    private void OnMouseDown()
    {
        var player = DummyPlayerController.Instance;

        if (player.TargetLocation != transform)
            player.TargetLocation = transform;
        else
            SceneManager.LoadScene("Scenes/" + SceneName);
    }
}
