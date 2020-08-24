using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Frogger
{
    public class FroggerBehavior : MonoBehaviour
    {
        [SerializeField] private int m_StartingHealth = 3;
        [SerializeField] private Sprite m_WaterSprite = default;

        private Animator m_Animator;
        private Tilemap m_Tilemap;
        private Vector3 m_Spawn;
        private SpriteRenderer[] m_Hearts;
        private int m_Health;
        private bool m_IsDead;
        private Transform m_Carrier;
        private MovingBehavior[] m_Behaviors;
        private Vector3 m_Input;

        public Transform Carrier
        {
            get => m_Carrier;
            set
            {
                m_Carrier = value;
                transform.SetParent(m_Carrier);
            }
        }

        public BoxCollider2D Box { get; private set; }

        private void Start()
        {
            Box = GetComponent<BoxCollider2D>();
            m_Animator = GetComponent<Animator>();
            m_Tilemap = FindObjectOfType<Tilemap>();
            m_Behaviors = FindObjectsOfType<MovingBehavior>();
            m_Hearts = GameObject.FindGameObjectsWithTag("Heart").Select(go => go.GetComponent<SpriteRenderer>()).ToArray();
            m_Spawn = transform.position;
            SetHearts(m_StartingHealth);
        }

        private void Update()
        {
            if (m_IsDead) return;

            Sprite sprite = null;
            Vector3Int? nextCellPosition = null;
            Transform t = transform;

            if (m_Input.sqrMagnitude > Mathf.Epsilon)
            {
                t.rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * Mathf.Atan2(-m_Input.x, m_Input.y), Vector3.forward);
                m_Animator.Play("Hop");

                nextCellPosition = m_Tilemap.layoutGrid.WorldToCell(t.position + m_Input);
                sprite = m_Tilemap.GetSprite(nextCellPosition.Value);
                if (sprite) t.position += m_Input;
                else nextCellPosition = null;
            }

            foreach (MovingBehavior behavior in m_Behaviors)
            {
                bool isIntersecting = Box.bounds.Intersects(behavior.Box.bounds);
                if (behavior.HandleFrogger(isIntersecting))
                {
                    if (behavior.IsKilling && isIntersecting) Kill();
                    else if (behavior.IsCarrying)
                    {
                        if (Carrier == behavior.transform) Carrier = null;
                        else if (isIntersecting) Carrier = behavior.transform;
                    }
                }
            }

            if (!Carrier && sprite == m_WaterSprite) Kill();

            if (!Carrier && nextCellPosition is Vector3Int cellPosition) t.position = m_Tilemap.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f);
        }

        private void LateUpdate() => m_Input = Vector3.zero;

        [UsedImplicitly]
        public void OnMove(InputAction.CallbackContext input)
        {
            if (input.started)
                m_Input = input.ReadValue<Vector2>();
        }

        private void DecrementHearts()
        {
            int health = m_Health - 1;
            IEnumerator DecrementEnumerator()
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
            StartCoroutine(DecrementEnumerator());
            SetHearts(health);
        }

        private void SetHearts(int health)
        {
            m_Health = health;
            for (var i = 0; i < m_StartingHealth; i++)
                m_Hearts[i].enabled = i < health;
        }

        private void Kill()
        {
            if (m_IsDead) return;

            DecrementHearts();
        }
    }
}