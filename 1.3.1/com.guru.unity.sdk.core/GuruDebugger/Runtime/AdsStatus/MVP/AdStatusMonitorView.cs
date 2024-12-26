
namespace Guru
{
    using System;
    using UnityEngine.EventSystems;

    using UnityEngine;
    using UnityEngine.UI;
    using Consts=AdStatusConsts;
    

    public class AdStatusMonitorView : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IPointerClickHandler
    {

        public bool Active
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }

        public float Alpha
        {
            get => _canvasGroup?.alpha ?? 0;
            set
            {
                if(_canvasGroup != null) 
                    _canvasGroup.alpha = value;
            } 
        }


        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Text _txtInfo;
        private IPointerClickHandler _pointerClickHandlerImplementation;


        private DateTime _clickStartDate;
        private bool _isOnDraging = false;

        public Action OnClickHandler;
        public Action OnDestryHandler;
        public Action<bool> OnEnableHandler;
        

        // Start is called before the first frame update
        void Awake()
        {
            Alpha = Consts.MonitorStartAlpha;
        }

        private void OnDestroy()
        {
            OnDestryHandler?.Invoke();
        }

        private void OnEnable()
        {
            OnEnableHandler?.Invoke(true);
        }


        private void OnDisable()
        {
            OnEnableHandler?.Invoke(false);
        }


        public void OnDrag(PointerEventData eventData)
        {
            var delat = eventData.delta;
            transform.localPosition += new Vector3(delat.x, delat.y, 0);
        }


        

        public void OnBeginDrag(PointerEventData eventData)
        {
            Alpha = 1;
            // _clickStartDate = DateTime.Now;
            _isOnDraging = true;
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            // var sp = DateTime.Now - _clickStartDate;
            // Debug.Log($"Drag Time: {sp.TotalSeconds}");
            _isOnDraging = false;
            OnPanelActivated();
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isOnDraging) return;
            // Debug.Log($"Click Handler delta: {eventData.delta}      isMoving: {eventData.IsPointerMoving()}");
            OnClickHandler?.Invoke();
            OnPanelActivated();
        }

        /// <summary>
        /// 数据刷新
        /// </summary>
        /// <param name="info"></param>
        public void OnUpdateInfo(string info)
        {
            _txtInfo.text = info;
        }
        
        private void OnPanelActivated()
        {
            Alpha = 1;
            CancelInvoke(nameof(OnbResetAlpha));
            Invoke(nameof(OnbResetAlpha), Consts.MonitorStayTime); // 自动置灰
        }


        private void OnbResetAlpha()
        {
            if (_canvasGroup == null) return;
            Alpha = Consts.MonitorStartAlpha;
        }

    }

}

