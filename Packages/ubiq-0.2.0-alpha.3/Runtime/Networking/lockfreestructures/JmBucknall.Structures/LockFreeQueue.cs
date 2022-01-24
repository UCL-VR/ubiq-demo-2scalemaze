using System;
using System.Collections.Generic;
using System.Text;
using Ubiq.Networking.JmBucknall.Threading;

namespace Ubiq.Networking
{
    namespace JmBucknall.Structures
    {
        public class LockFreeQueue<T>
        {

            SingleLinkNode<T> head;
            SingleLinkNode<T> tail;

            public LockFreeQueue()
            {
                head = new SingleLinkNode<T>();
                tail = head;
            }

            public void Enqueue(T item)
            {
                SingleLinkNode<T> oldTail = null;
                SingleLinkNode<T> oldTailNext;

                SingleLinkNode<T> newNode = new SingleLinkNode<T>();
                newNode.Item = item;

                bool newNodeWasAdded = false;
                while (!newNodeWasAdded)
                {
                    oldTail = tail;
                    oldTailNext = oldTail.Next;

                    if (tail == oldTail)
                    {
                        if (oldTailNext == null)
                            newNodeWasAdded = SyncMethods.CAS<SingleLinkNode<T>>(ref tail.Next, null, newNode);
                        else
                            SyncMethods.CAS<SingleLinkNode<T>>(ref tail, oldTail, oldTailNext);
                    }
                }

                SyncMethods.CAS<SingleLinkNode<T>>(ref tail, oldTail, newNode);
            }

            public bool Dequeue(out T item)
            {
                item = default(T);
                SingleLinkNode<T> oldHead = null;

                bool haveAdvancedHead = false;
                while (!haveAdvancedHead)
                {

                    oldHead = head;
                    SingleLinkNode<T> oldTail = tail;
                    SingleLinkNode<T> oldHeadNext = oldHead.Next;

                    if (oldHead == head)
                    {
                        if (oldHead == oldTail)
                        {
                            if (oldHeadNext == null)
                            {
                                return false;
                            }
                            SyncMethods.CAS<SingleLinkNode<T>>(ref tail, oldTail, oldHeadNext);
                        }

                        else
                        {
                            item = oldHeadNext.Item;
                            haveAdvancedHead =
                              SyncMethods.CAS<SingleLinkNode<T>>(ref head, oldHead, oldHeadNext);
                        }
                    }
                }
                return true;
            }

            public T Dequeue()
            {
                T result;
                Dequeue(out result);
                return result;
            }
        }
    }
}