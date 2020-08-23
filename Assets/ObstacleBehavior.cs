using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ObstacleBehavior : MonoBehaviour
{
    private BoxCollider2D m_Box;
    private FroggerBehavior m_Frogger;

    [SerializeField] private ContactFilter2D m_Filter = default;

    private void Start()
    {
        m_Box = GetComponent<BoxCollider2D>();
        m_Frogger = FindObjectOfType<FroggerBehavior>();
    }

    private void Update()
    {
        if (!m_Box.IsTouching(m_Frogger.Box, m_Filter)) return;

        m_Frogger.OnHit();
    }
}