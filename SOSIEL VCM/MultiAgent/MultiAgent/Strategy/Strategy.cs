using System;

namespace MultiAgent
{
    public class Strategy<T>
    {
        public T IfCondition { get; set; }

        public T ThenCondition { get; set; }

        public T ElseCondition { get; set; }
    }
}