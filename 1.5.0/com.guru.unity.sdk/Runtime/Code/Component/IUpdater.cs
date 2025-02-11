namespace Guru
{
    /// <summary>
    /// 更新器
    /// </summary>
    public interface IUpdater
    {
        UpdaterState State { get; }
        void Start();
        void OnUpdate();
        void Pause(bool pause);
        void Kill();
    }


    public enum UpdaterState
    {
        Prepare,
        Running,
        Pause,
        Kill,
    }
}