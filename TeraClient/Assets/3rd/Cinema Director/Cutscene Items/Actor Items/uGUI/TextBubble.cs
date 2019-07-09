using UnityEngine;
using UnityEngine.UI;

namespace CinemaDirector
{
    public class TextBubble : MonoBehaviour
    {
        public float Duration = 2F;
        [HideInInspector]
        public Vector3 DeltaHeight = Vector3.zero;
        [HideInInspector]
        public Image image;
        [HideInInspector]
        public Text text;
        public Transform target;

        private const int FONT_COUNT = 10;
        private float _startWidth = 30F;
        private float _deltaWidth = 15F;
        private float _startHeight = 10F;
        private float _deltaHeight = 15F;

        public void OnInit(int num)
        {
            int length = num > FONT_COUNT ? FONT_COUNT : num;
            int multi = Mathf.CeilToInt((float)num / FONT_COUNT);
            DeltaHeight = new Vector3(0, 1.7F + multi * 0.2F, 0);
            //考虑空隙
            multi = multi * 2 - 1;
            float width = _startWidth + (length - 1) * _deltaWidth;
            float height = _startHeight + multi * _deltaHeight;
            image.rectTransform.sizeDelta = new Vector2(width, height);
            text.rectTransform.sizeDelta = new Vector2(width, text.rectTransform.sizeDelta.y);
            gameObject.SetActive(true);
            CancelInvoke();
            Invoke("OnHide", Duration);
        }

        private void OnHide()
        {
            gameObject.SetActive(false);
        }

        void Update()
        {
            CGGlobal cgg = GetComponentInParent<CGGlobal>();
            if (!target || !cgg.Current)
                return;
            transform.position = target.position + DeltaHeight;
            transform.rotation = cgg.Current.transform.rotation;
        }
    }
}
