using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EchoShift.Presentation
{
    public sealed class Phase4PauseButtonVisual : MonoBehaviour,
        ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Outline border;
        [SerializeField] private GameObject arrow;
        [SerializeField] private GameObject selectionFill;

        public bool HasRequiredReferences => border != null && arrow != null && selectionFill != null;
        public bool IsHighlighted => arrow != null && arrow.activeSelf;
        public bool IsSelectionFillVisible => selectionFill != null && selectionFill.activeSelf;

        public void Configure(
            Outline selectionBorder, GameObject selectionArrow, GameObject selectedFill)
        {
            border = selectionBorder;
            arrow = selectionArrow;
            selectionFill = selectedFill;
            SetHighlighted(false);
        }

        public void OnSelect(BaseEventData eventData) => SetHighlighted(true);
        public void OnDeselect(BaseEventData eventData) => SetHighlighted(false);
        public void OnPointerEnter(PointerEventData eventData) => SetHighlighted(true);
        public void OnPointerExit(PointerEventData eventData)
        {
            if (EventSystem.current == null ||
                EventSystem.current.currentSelectedGameObject != gameObject)
                SetHighlighted(false);
        }

        private void SetHighlighted(bool highlighted)
        {
            if (border != null) border.enabled = highlighted;
            if (arrow != null) arrow.SetActive(highlighted);
            if (selectionFill != null) selectionFill.SetActive(highlighted);
        }
    }
}
