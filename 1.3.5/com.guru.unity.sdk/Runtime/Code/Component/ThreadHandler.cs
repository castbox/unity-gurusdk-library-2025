


namespace Guru
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    
    public class ThreadHandler: IUpdater
    {
        private Queue<Action> _actions;
        public Queue<Action> Actions
        {
            get
            {
                if(_actions == null) 
                    _actions = new Queue<Action>(10);
                return _actions;
            }

            set
            {
                if (value != null) _actions = value;
            }
        }

        private UpdaterState _state;
        public UpdaterState State => _state;


        /// <summary>
        /// 启动 Updater
        /// </summary>
        public void Start()
        {
            _state = UpdaterState.Running;
        }

        /// <summary>
        /// 执行方案
        /// </summary>
        public void OnUpdate()
        {
            if (Actions.Count > 0)
            {
                // 消耗对垒
                while (Actions.Count > 0)
                {
                    Actions.Dequeue()?.Invoke();
                }
            }
        }
        
        public void Pause(bool pause = true)
        {
            _state = pause ? UpdaterState.Pause : UpdaterState.Running;
        }
        
        /// <summary>
        /// 删除对象
        /// </summary>
        public void Kill()
        {
            _state = UpdaterState.Kill;
        }

        public void Dispose()
        {
            _actions.Clear();
            _state = UpdaterState.Kill;
        }
        
        public void AddAction(Action action)
        {
            if (action == null) return;
            Actions.Enqueue(action);
        }
        
    }
}