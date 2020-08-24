using System.Collections;
using TMPro;
using UnityEngine;

namespace Frogger
{
    public class InterfaceBehavior : MonoBehaviour
    {
        [SerializeField] private float m_InterfaceSeconds = 4.0f;
        
        public static IEnumerator SetText(string text)
        {
            var instance = FindObjectOfType<InterfaceBehavior>(true);
            instance.gameObject.SetActive(true);
            instance.GetComponentInChildren<TextMeshProUGUI>().SetText(text);
            yield return new WaitForSeconds(instance.m_InterfaceSeconds);
            instance.gameObject.SetActive(false);
        }
    }
}