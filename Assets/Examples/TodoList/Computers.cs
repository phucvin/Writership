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
            IOp<string> newItem_,
            IOp<Empty> deleteCompletedItems_,
            IOp<string> deleteItem_,
            ITodoItemFactory factory)
        {
            var newItem = newItem_.Read();
            var deleteCompletedItems = deleteCompletedItems_.Read();
            var deleteItem = deleteItem_.Read();

            if (newItem.Count <= 0 &&
                deleteCompletedItems.Count <= 0 &&
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

            if (deleteCompletedItems.Count > 0)
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

            if (deleteItem.Count > 0)
            {
                items.RemoveAll(it =>
                {
                    if (deleteItem.Contains(it.Id))
                    {
                        it.Dispose();
                        return true;
                    }
                    return false;
                });
            }
        }

        public static void EditingItemId(IEl<string> target, IOp<string> edit_, IOp<string> finish)
        {
            var edit = edit_.Read();
            var editingItemId = target.Read();

            if (finish.Read().Count > 0) editingItemId = null;
            else if (edit.Count > 0) editingItemId = edit[edit.Count - 1];

            if (editingItemId != target.Read()) target.Write(editingItemId);
        }

        public static class TodoItem
        {
            public static void IsCompleted(IEl<bool> target, IOp<string> toggle_, string myId)
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
                var finishEdit = finishEdit_.Read();

                if (editingItemId.Read() == myId && finishEdit.Count > 0)
                {
                    string newContent = finishEdit[finishEdit.Count - 1];
                    if (!string.IsNullOrEmpty(newContent)) target.Write(newContent);
                }
            }
        }
    }
}