using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuFocus : MonoBehaviour
{
    [SerializeField] Selectable firstSelected;

    GameObject last;

    void OnEnable()
    {
        last = null;
        StartCoroutine(selectNextFrame());
    }

    IEnumerator selectNextFrame()
    {
        yield return null;
        if (firstSelected != null) focus(firstSelected);
    }

    public void focus(Selectable button)
    {
        if (button == null || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
        last = button.gameObject;
    }

    void Update()
    {
        EventSystem es = EventSystem.current;
        if (es == null) return;
        GameObject current = es.currentSelectedGameObject;
        if (current != null && current.activeInHierarchy) { last = current; return; }

        bool navKey = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)
                   || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)
                   || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S)
                   || Input.GetKeyDown(KeyCode.Return);
        if (!navKey) return;

        Selectable target = last != null && last.activeInHierarchy ? last.GetComponent<Selectable>() : firstSelected;
        if (target != null && target.IsInteractable()) focus(target);
    }
}
