using System;
using System.Collections.Generic;
using Writership;

namespace Examples.Inventory
{
    public static class ListExtensions
    {
        public static void RemoveExact<T>(this List<T> list, T item)
        {
            if (!list.Remove(item)) throw new NotImplementedException();
        }

        public static void RemoveExact<T>(this List<T> list, IList<T> items)
        {
            int oldCount = list.Count;
            list.RemoveAll(it => items.Contains(it));
            if (list.Count - oldCount != items.Count) throw new NotImplementedException();
        }
    }

    public class State
    {
        public readonly Data Data;
        public readonly Profile Profile;

        public readonly Home Home;
        public readonly Inventory Inventory;

        public void Setup(CompositeDisposable cd, IEngine engine)
        {
            engine.Worker(cd, Dep.On(
                Inventory.SellItem.Yes, Inventory.SellItems.Yes,
                Inventory.FuseItem.Yes), () =>
            {
                var items = Profile.Items.AsWriteProxy();
                if (Inventory.SellItem.Yes)
                {
                    items.RemoveExact(Inventory.ViewingItem);
                }
                if (Inventory.SellItems.Yes)
                {
                    items.RemoveExact(Inventory.SellingItems.Read());
                }
                if (Inventory.FuseItem.Yes)
                {
                    items.Add(Inventory.FuseItemResult);
                }
                items.Commit();
            });
        }
    }

    public class Data
    {
        public readonly IList<int> ItemUpgradeRequiredGoldByLevel;
    }

    public class Profile
    {
        public readonly El<int> Gold;
        public readonly Li<Item> Items;
    }

    public class Item
    {
        public readonly El<int> Id;
        public readonly string Name;
        public readonly El<int> Level;
    }

    public class Home
    {
        public readonly Op<Empty> OpenInventory;
    }

    public class Inventory
    {
        public readonly Li<Item> SortedItems;
        public readonly Op<int> ChangeSort;

        public readonly El<Item> ViewingItem;
        public readonly VerifyOp<Empty> UpgradeItem;
        public readonly ConfirmOp<Empty> SellItem;

        public readonly El<Item> FuseItemA;
        public readonly El<Item> FuseItemB;
        public readonly El<Item> FuseItemResult;
        public readonly VerifyConfirmOp<Empty> FuseItem;

        public readonly Li<Item> SellingItems;
        public readonly ConfirmOp<Empty> SellItems;
    }

    public class ConfirmOp<T>
    {
        public readonly Op<T> Trigger;
        public readonly Op<T> Yes;
        public readonly Op<T> No;
    }

    public class VerifyOp<T>
    {
        public readonly El<bool> Status;
        public readonly Op<T> Trigger;
        public readonly Op<T> Verified;
        public readonly Op<T> Rejected;
    }

    public class VerifyConfirmOp<T>
    {
        public readonly El<bool> Status;
        public readonly Op<T> Trigger;
        public readonly Op<T> Yes;
        public readonly Op<T> No;
        public readonly Op<T> Rejected;
    }
}