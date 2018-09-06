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

        public static string EditingItem(IList<string> opEdit, IList<string> opFinish)
        {
            if (opFinish.Count > 0) return null;
            else if (opEdit.Count > 0) return opEdit[opEdit.Count - 1];
            else return null;
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

            public static string Content(string content, string editingItem, IList<string> opFinishEdit, string myId)
            {
                if (editingItem == myId && opFinishEdit.Count > 0)
                {
                    string newContent = opFinishEdit[opFinishEdit.Count - 1];
                    if (!string.IsNullOrEmpty(newContent)) return newContent;
                }

                return content;
            }
        }
    }
}