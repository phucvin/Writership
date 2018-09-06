using System.Collections.Generic;
using Writership;

namespace Examples.TodoList
{
    public static class Computers
    {
        public static int UncompletedCount(IList<TodoList.TodoItem> items)
        {
            int uncompletedCount = 0;
            for (int i = 0, n = items.Count; i < n; ++i)
            {
                if (!items[i].IsCompleted.Read()) ++uncompletedCount;
            }
            return uncompletedCount;
        }

        public static void Items(
            List<TodoList.TodoItem> items,
            El<int> nextId,
            IList<string> opNew,
            IList<Empty> opDeleteCompleted,
            IList<string> opDelete,
            TodoList.TodoItem.Factory factory
        )
        {
            if (opNew.Count > 0)
            {
                for (int i = 0, n = opNew.Count; i < n; ++i)
                {
                    items.Add(factory.Create((nextId.Read() + i).ToString(), opNew[i]));
                }
                nextId.Write(nextId.Read() + opNew.Count);
            }

            if (opDeleteCompleted.Count > 0)
            {
                items.RemoveAll(it =>
                {
                    if (it.IsCompleted.Read())
                    {
                        it.Dispose();
                        return true;
                    }
                    return false;
                });
            }

            if (opDelete.Count > 0)
            {
                items.RemoveAll(it =>
                {
                    if (opDelete.Contains(it.Id))
                    {
                        it.Dispose();
                        return true;
                    }
                    return false;
                });
            }
        }

        public static class TodoItem
        {
            public static bool IsCompleted(bool isCompleted, IList<string> opToggle, string myId)
            {
                for (int i = 0, n = opToggle.Count; i < n; ++i)
                {
                    if (opToggle[i] == myId) isCompleted = !isCompleted;
                }
                return isCompleted;
            }
        }
    }
}