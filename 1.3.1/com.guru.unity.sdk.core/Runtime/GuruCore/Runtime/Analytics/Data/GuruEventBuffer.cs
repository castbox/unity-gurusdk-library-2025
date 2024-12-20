namespace Guru
{
    using System.Collections.Concurrent;
    public class GuruEventBuffer<T> 
    {
        private readonly ConcurrentQueue<T> _buffer;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public GuruEventBuffer()
        {
            _buffer = new ConcurrentQueue<T>();
        }
        
        /// <summary>
        /// 入栈
        /// </summary>
        /// <param name="item"></param>
        public void Push(T item)
        {
            _buffer.Enqueue(item);
        }
        
        /// <summary>
        /// 出栈
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Pop(out T item)
        {
            return _buffer.TryDequeue(out item);
        }
    }




    
        
    
    
    
}