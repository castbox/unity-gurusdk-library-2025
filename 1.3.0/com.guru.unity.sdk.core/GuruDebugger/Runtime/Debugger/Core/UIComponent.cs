using System;
using UnityEngine;

namespace Guru
{
    public class UIComponent: MonoBehaviour
    {
        
        public virtual long GID { get; protected set; }
        public virtual string ID => $"{GID}";
        
        public Transform Parent
        {
            get => transform.parent;
            set
            {
                transform.SetParent(value);
                transform.localPosition = Vector3.zero;
            }
        }

        public Vector2 Size
        {
            get => _rectTransform.sizeDelta;
            set => _rectTransform.sizeDelta = value;
        }


        public bool Active
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }

        protected RectTransform _rectTransform;
        private void Awake()
        {
            _rectTransform = gameObject.GetComponent<RectTransform>();

            OnCreated();
        }

        protected virtual void OnCreated()
        {
            
        }

        /// <summary>
        /// 刷新UI
        /// </summary>
        public virtual void Refresh()
        {
            
        }

    }
}