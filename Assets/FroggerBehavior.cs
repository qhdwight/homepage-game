using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class FroggerBehavior : MonoBehaviour
{
    private Animator m_Animator;
    private Tilemap m_Tilemap;
    private Vector3 m_Spawn;

    public BoxCollider2D Box { get; private set; }

    private void Start()
    {
        Box = GetComponent<BoxCollider2D>();
        m_Animator = GetComponent<Animator>();
        m_Tilemap = FindObjectOfType<Tilemap>();
        m_Spawn = transform.position;
    }

    [UsedImplicitly]
    public void OnMove(InputValue input)
    {
        Vector3 move = input.Get<Vector2>();
        if (move.sqrMagnitude < Mathf.Epsilon) return;

        Transform t = transform;
        t.rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * Mathf.Atan2(-move.x, move.y), Vector3.forward);
        Vector3 position = t.position + move;
        m_Animator.Play("Hop");

        Vector3Int cellPosition = m_Tilemap.layoutGrid.WorldToCell(position);
        t.position = m_Tilemap.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f, 0.5f);
        Sprite sprite = m_Tilemap.GetSprite(cellPosition);
    }

    public void OnHit() { transform.SetPositionAndRotation(m_Spawn, Quaternion.identity); }
}