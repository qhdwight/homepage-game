using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class FroggerBehavior : MonoBehaviour
{
    private Animator m_Animator;
    private Tilemap m_Tilemap;
    private Vector3 m_Spawn;
    private SpriteRenderer[] m_Hearts;
    private int m_Health;
    private bool m_IsDead;

    [SerializeField] private int m_StartingHealth = 3;

    public BoxCollider2D Box { get; private set; }

    private void Start()
    {
        Box = GetComponent<BoxCollider2D>();
        m_Animator = GetComponent<Animator>();
        m_Tilemap = FindObjectOfType<Tilemap>();
        m_Hearts = GameObject.FindGameObjectsWithTag("Heart").Select(go => go.GetComponent<SpriteRenderer>()).ToArray();
        m_Spawn = transform.position;
        SetHealth(m_StartingHealth);
    }

    [UsedImplicitly]
    public void OnMove(InputValue input)
    {
        if (m_IsDead) return;
        
        Vector3 move = input.Get<Vector2>();
        if (move.sqrMagnitude < Mathf.Epsilon) return;

        Transform t = transform;
        t.rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * Mathf.Atan2(-move.x, move.y), Vector3.forward);
        m_Animator.Play("Hop");

        Vector3Int cellPosition = m_Tilemap.layoutGrid.WorldToCell(t.position + move);
        Vector3 snappedPosition = m_Tilemap.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f);

        Sprite sprite = m_Tilemap.GetSprite(cellPosition);
        if (!sprite) return;

        t.position = snappedPosition;
    }

    private void DecrementHealth()
    {
        int health = m_Health - 1;
        IEnumerator Load()
        {
            m_IsDead = true;
            m_Animator.Play("Die");
            yield return new WaitForSeconds(1.0f);
            if (health == 0) yield return SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            else
            {
                m_Animator.Play("Idle");
                m_IsDead = false;
                transform.SetPositionAndRotation(m_Spawn, Quaternion.identity);
            }
        }
        StartCoroutine(Load());
        SetHealth(health);
    }

    private void SetHealth(int health)
    {
        m_Health = health;
        for (var i = 0; i < m_StartingHealth; i++)
            m_Hearts[i].enabled = i < health;
    }

    public void OnHit()
    {
        if (m_IsDead) return;
        
        DecrementHealth();
    }
}