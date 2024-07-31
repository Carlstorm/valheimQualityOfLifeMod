using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomUITooltip : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
    public Selectable m_selectable;

    public GameObject m_tooltipPrefab;

    public RectTransform m_anchor;

    public Vector2 m_fixedPosition;

    public string m_text = "";

    public string m_topic = "";

    public GameObject m_gamepadFocusObject;

    public static CustomUITooltip m_current;

    public static GameObject m_tooltip;

    public static GameObject m_hovered;

    public const float m_showDelay = 0.5f;

    public float m_showTimer;

    private bool m_isSlotHovered;
    public void Awake()
    {
        m_selectable = GetComponent<Selectable>();
    }

    public void LateUpdate()
    {
        if (m_current == this && !m_tooltip.activeSelf)
        {
            m_showTimer += Time.deltaTime;
            if (m_showTimer > 0.5f || (ZInput.IsGamepadActive() && !ZInput.IsMouseActive()))
            {
                m_tooltip.SetActive(value: true);
            }
        }

        if (ZInput.IsGamepadActive() && !ZInput.IsMouseActive())
        {
            if (m_gamepadFocusObject != null)
            {
                if (m_gamepadFocusObject.activeSelf && m_current != this)
                {
                    OnHoverStart(m_gamepadFocusObject);
                }
                else if (!m_gamepadFocusObject.activeSelf && m_current == this)
                {
                    HideTooltip();
                }
            }
            else if ((bool)m_selectable)
            {
                if (EventSystem.current.currentSelectedGameObject == m_selectable.gameObject && m_current != this)
                {
                    OnHoverStart(m_selectable.gameObject);
                }
                else if (EventSystem.current.currentSelectedGameObject != m_selectable.gameObject && m_current == this)
                {
                    HideTooltip();
                }
            }

            if (m_current == this && m_tooltip != null)
            {
                if (m_anchor != null)
                {
                    m_tooltip.transform.SetParent(m_anchor);
                    m_tooltip.transform.localPosition = m_fixedPosition;
                    return;
                }

                if (m_fixedPosition != Vector2.zero)
                {
                    m_tooltip.transform.position = m_fixedPosition;
                    return;
                }

                RectTransform obj = base.gameObject.transform as RectTransform;
                Vector3[] array = new Vector3[4];
                obj.GetWorldCorners(array);
                m_tooltip.transform.position = (array[1] + array[2]) / 2f;
                Utils.ClampUIToScreen(m_tooltip.transform.GetChild(0).transform as RectTransform);
            }
        }
        else if (m_current == this)
        {
            if (m_hovered == null)
            {
                HideTooltip();
                return;
            }

            if (m_tooltip.activeSelf && !RectTransformUtility.RectangleContainsScreenPoint(m_hovered.transform as RectTransform, ZInput.mousePosition))
            {
                HideTooltip();
                return;
            }

            Jotunn.Logger.LogWarning("made it to here");
            m_tooltip.transform.position = ZInput.mousePosition;
            Utils.ClampUIToScreen(m_tooltip.transform.GetChild(0).transform as RectTransform);
        }
    }

    public void OnDisable()
    {
        if (m_current == this)
        {
            HideTooltip();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //OnHoverStart(eventData.pointerEnter);
    }

    public void OnHoverStart(GameObject go)
    {
        if ((bool)m_current)
        {
            HideTooltip();
        }

        if (m_tooltip == null && (m_text != "" || m_topic != ""))
        {
            m_tooltip = Object.Instantiate(m_tooltipPrefab, base.transform.GetComponentInParent<Canvas>().transform);
            UpdateTextElements();
            Utils.ClampUIToScreen(m_tooltip.transform.GetChild(0).transform as RectTransform);
            m_hovered = go;
            m_current = this;
            m_tooltip.SetActive(value: false);
            m_showTimer = 0f;
        }
    }

    public void UpdateTextElements()
    {
        if (m_tooltip != null)
        {
            Transform transform = Utils.FindChild(m_tooltip.transform, "Text");
            if (transform != null)
            {
                transform.GetComponent<TMP_Text>().text = Localization.instance.Localize(m_text);
            }

            Transform transform2 = Utils.FindChild(m_tooltip.transform, "Topic");
            if (transform2 != null)
            {
                transform2.GetComponent<TMP_Text>().text = Localization.instance.Localize(m_topic);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (m_current == this)
        {
            HideTooltip();
        }
    }

    public static void HideTooltip()
    {
        if ((bool)m_tooltip)
        {
            Object.Destroy(m_tooltip);
            m_current = null;
            m_tooltip = null;
            m_hovered = null;
        }
    }

    public void OnSlotHoverExit()
    {
        m_isSlotHovered = false;
        HideTooltip();
    }

    public void OnSlotHoverEnter(GameObject slot, string topic, string text)
    {
        m_isSlotHovered = true;
        m_hovered = slot;

        if (m_tooltip == null)
        {
            m_tooltip = Instantiate(m_tooltipPrefab, transform.GetComponentInParent<Canvas>().transform);
            UpdateTextElements(topic, text);
            Utils.ClampUIToScreen(m_tooltip.transform.GetChild(0).transform as RectTransform);
            m_current = this;
            m_tooltip.SetActive(false);
            m_showTimer = 0f;
        }
        else
        {
            UpdateTextElements(topic, text);
            m_tooltip.SetActive(true);
        }
    }

    private void UpdateTextElements(string topic, string text)
    {
        if (m_tooltip != null)
        {
            Transform transform = Utils.FindChild(m_tooltip.transform, "Text");
            if (transform != null)
            {
                transform.GetComponent<TMP_Text>().text = Localization.instance.Localize(text);
            }

            Transform transform2 = Utils.FindChild(m_tooltip.transform, "Topic");
            if (transform2 != null)
            {
                transform2.GetComponent<TMP_Text>().text = Localization.instance.Localize(topic);
            }
        }
    }

    public void Set(string topic, string text, RectTransform anchor = null, Vector2 fixedPosition = default(Vector2))
    {
        m_anchor = anchor;
        m_fixedPosition = fixedPosition;
        if (topic == m_topic && text == m_text)
        {
            return;
        }

        m_topic = topic;
        m_text = text;
        if (m_current == this && m_tooltip != null)
        {
            UpdateTextElements();
        }
        else
        {
            if (!(m_selectable != null) || ZInput.instance == null)
            {
                return;
            }

            RectTransform obj = m_selectable.transform as RectTransform;
            Vector3 point = obj.InverseTransformPoint(ZInput.mousePosition);
            if (obj.rect.Contains(point))
            {
                List<RaycastResult> list = new List<RaycastResult>();
                PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
                pointerEventData.position = ZInput.mousePosition;
                EventSystem.current.RaycastAll(pointerEventData, list);
                if (list.Count > 0 && list[0].gameObject == m_selectable.gameObject)
                {
                    OnHoverStart(m_selectable.gameObject);
                }
            }
        }
    }
}