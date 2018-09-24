using Writership;

namespace Examples.TodoList
{
    public static class Computers
    {
        public static void UncompletedCount(IEl<int> target, ILi<ITodoItem> items_)
        {
            var items = items_.Read();
            int uncompletedCount = 0;
            for (int i = 0, n = items.Count; i < n; ++i)
            {
                if (!items[i].IsCompleted.Read()) ++uncompletedCount;
            }
            if (uncompletedCount != target.Read()) target.Write(uncompletedCount);
        }

        public static void Items(
            ILi<ITodoItem> target,
            IEl<int> nextId,
            IMultiOp<string> newItem_,
            IOp<Empty> deleteCompletedItems_,
            IMultiOp<string> deleteItem_,
            ITodoItemFactory factory)
        {
            var newItem = newItem_.Read();
            Empty tmp;
            bool deleteCompletedItems = deleteCompletedItems_.TryRead(out tmp);
            var deleteItem = deleteItem_.Read();

            if (newItem.Count <= 0 &&
                !deleteCompletedItems &&
                deleteItem.Count <= 0)
            {
                return;
            }

            var items = target.AsWrite();

            if (newItem.Count > 0)
            {
                for (int i = 0, n = newItem.Count; i < n; ++i)
                {
                    items.Add(factory.Create((nextId.Read() + i).ToString(), newItem[i]));
                }
                nextId.Write(nextId.Read() + newItem.Count);
            }

            if (deleteCompletedItems)
            {
                items.RemoveAll(it =>
                {
                    if (it.IsCompleted.Read())
                    {
                        factory.Dispose(it);
                        return true;
                    }
                    return false;
                });
            }

            if (deleteItem.Count > 0)
            {
                items.RemoveAll(it =>
                {
                    if (deleteItem.Contains(it.Id))
                    {
                        factory.Dispose(it);
                        return true;
                    }
                    return false;
                });
            }
        }

        public static void EditingItemId(IEl<string> target, IOp<string> edit_, IOp<string> finish)
        {
            string edit;
            string tmp;
            var editingItemId = target.Read();

            if (finish.TryRead(out tmp)) editingItemId = null;
            else if (edit_.TryRead(out edit)) editingItemId = edit;

            if (editingItemId != target.Read()) target.Write(editingItemId);
        }

        public static class TodoItem
        {
            public static void IsCompleted(IEl<bool> target, IMultiOp<string> toggle_, string myId)
            {
                var toggle = toggle_.Read();

                var isCompleted = target.Read();
                for (int i = 0, n = toggle.Count; i < n; ++i)
                {
                    if (toggle[i] == myId) isCompleted = !isCompleted;
                }
                if (isCompleted != target.Read()) target.Write(isCompleted);
            }

            public static void Content(IEl<string> target, IEl<string> editingItemId, IOp<string> finishEdit_, string myId)
            {
                string finishEdit;
                if (editingItemId.Read() == myId && finishEdit_.TryRead(out finishEdit))
                {
                    string newContent = finishEdit;
                    if (!string.IsNullOrEmpty(newContent)) target.Write(newContent);
                }
            }
        }
    }
}