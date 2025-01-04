namespace Guru
{
    /// <summary>
    /// Mono单例类型
    /// </summary>
    public enum EMonoSingletonType
    {
        /// <summary>
        /// 已经存在在场景中
        /// </summary>
        ExitsInScene,

        /// <summary>
        /// 从Resources下加载
        /// </summary>
        LoadedFromResources,

        /// <summary>
        /// 创建一个新GameObject挂载脚本实现单例
        /// </summary>
        CreateOnNewGameObject,
    }
}