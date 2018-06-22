using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataMining.Core
{
    public class FPGrowthMining
    {

        private static ItemsetCollection ParallelFPGrowth(ItemsetCollection db, double supportThreshold,ref ItemsetCollection F)
        {
            ItemsetCollection Fre = new ItemsetCollection();//List of Items > support
            Dictionary<int, int> ItemCount = new Dictionary<int, int>();
            Dictionary<int, int> ItemCount1 = new Dictionary<int, int>();
            ItemsetCollection T = new ItemsetCollection();//frequent items
            ItemsetCollection T1 = new ItemsetCollection();
            int n = db.Count / 2;
            Thread t1 = new Thread(() =>
            {
               for(int i=0;i<n;i++)
                {
                    foreach(int item in db[i])
                    {
                        if(ItemCount1.ContainsKey(item))
                        {
                            ItemCount1[item] += 1;
                        }
                        else
                        {
                            ItemCount1.Add(item, 1);
                        }
                    }
                }
            });
            t1.Start();
            Thread t2 = new Thread(() =>
            {
                for (int j = n; j < db.Count; j++)
                {
                    foreach (int item in db[j])
                    {
                        if (ItemCount.ContainsKey(item))
                        {
                            ItemCount[item] += 1;
                        }
                        else
                        {
                            ItemCount.Add(item, 1);
                        }
                    }
                }
            });
            t2.Start();
            t1.Join();
            t2.Join();
            foreach (KeyValuePair<int,int> keyvalue in ItemCount1)
            {
                if(ItemCount.ContainsKey(keyvalue.Key))
                {
                    ItemCount[keyvalue.Key] += keyvalue.Value;
                }
                else
                {
                    ItemCount.Add(keyvalue.Key,keyvalue.Value);
                }
            }
            double count = db.Count;
            foreach (KeyValuePair<int, int> keyvalue in ItemCount)
            {
                Itemset itemset = new Itemset() { keyvalue.Key };
                itemset.Support = ((double)keyvalue.Value / count) * 100.0;
                if (itemset.Support >= supportThreshold)
                {
                    Fre.Add(itemset);
                }
            }
            Fre.Sort(delegate (Itemset x, Itemset y)
            {
                if (x.Support > y.Support) return -1;
                else if (x.Support < y.Support)
                    return 1;
                else return 0;
            });
            F = Fre;
            Thread t11 = new Thread(() =>
            {
                for (int i = 0; i < n; i++)
                {
                    Itemset itemset = new Itemset();
                    foreach (Itemset itemFre in Fre)
                    {
                        if (db[i].Contains(itemFre[0]))
                        {
                            itemset.Add(itemFre[0]);
                        }
                    }
                    if (itemset.Count > 0)
                        T1.Add(itemset);
                }
            });
            t11.Start();
            Thread t12 = new Thread(() =>
            {
                for (int i = n; i < db.Count; i++)
                {
                    Itemset itemset = new Itemset();
                    foreach (Itemset itemFre in Fre)
                    {
                        if (db[i].Contains(itemFre[0]))
                        {
                            itemset.Add(itemFre[0]);
                        }
                    }
                    if (itemset.Count > 0)
                        T.Add(itemset);
                }
            });
            t12.Start();
            t11.Join();
            t12.Join();
            foreach (var item in T1)
                T.Add(item);
            return T;
        }
        public static ItemsetCollection DoFPGrowth(ItemsetCollection db, double supportThreshold)
        {
           
            
            ItemsetCollection Fre = new ItemsetCollection();//List of Items > support
            ItemsetCollection T = new ItemsetCollection();//frequent items
            List<ItemsetCollection> P = new List<ItemsetCollection>();
            ItemsetCollection FPTreeCon = new ItemsetCollection();//Conditional FP-Tree
            //item > support
            Itemset I = db.GetUniqueItems();
            foreach (int item in I)
            {
                Itemset itemset = new Itemset() { item };
                itemset.Support = db.FindSupport(itemset);
                if (itemset.Support >= supportThreshold)
                {
                    Fre.Add(itemset);
                }
            }
            // sort item
            Fre.Sort(delegate (Itemset x, Itemset y)
            {
                if (x.Support > y.Support) return -1;
                else if (x.Support < y.Support)
                    return 1;
                else return 0;
            });
            //Fre = ParallelFre(db, supportThreshold);
            //frequent items
            foreach (Itemset itemdata in db)
            {
                Itemset itemset = new Itemset();
                foreach (Itemset itemFre in Fre)
                {
                    if (itemdata.Contains(itemFre[0]))
                    {
                        itemset.Add(itemFre[0]);
                    }
                }
                T.Add(itemset);
            }

            //T = ParallelFPGrowth(db, supportThreshold,ref Fre);
            // create FP Tree
            FPTree tree = CreateTree(T);
            
            // Conditional Patern Bases
            foreach (Itemset itemset in Fre)
            {
                ItemsetCollection itemsetCollection = new ItemsetCollection();
                for (int j = 0; j < tree.countNode; j++)
                {
                    var node = tree.arrayNode[j];
                    if (node.itemName.Equals(itemset[0]) && !node.visited)
                    {
                        node.visited = true;
                        var nodeparent = node.nodeParent;                      
                        while (nodeparent.itemName >-1)
                        {
                            int index = itemsetCollection.FindIndex(nodeparent.itemName);
                            if(index>-1)
                            {
                                itemsetCollection[index].Support += node.count;
                            }
                            else
                            {
                                Itemset item = new Itemset();
                                item.Support = node.count;
                                item.Add(nodeparent.itemName);
                                itemsetCollection.Add(item);
                            }
                            nodeparent = nodeparent.nodeParent;
                        }
                    }
                }
                P.Add(itemsetCollection);
            }
            // Conditional FP-Tree
            for(int i=0;i< P.Count;i++)
            {
                ItemsetCollection itemsetCollection = new ItemsetCollection();
                //itemsetCollection.Add(Fre[i]); //hoán vị sau
                foreach(var item in P[i])
                {
                    if(db.Support(item)>= supportThreshold)
                    {
                        itemsetCollection.Add(item);
                    }
                }
                ItemsetCollection subsets = Bit.FindSubsets(itemsetCollection.GetUniqueItems(), 0); //get all subsets
                foreach(Itemset items in subsets)
                {
                    items.Add(Fre[i][0]);
                    items.Support = db.FindSupport(items);
                    if (items.Support >= supportThreshold)
                        FPTreeCon.Add(items);
                }
            }
            return (FPTreeCon);
        }
        public static ItemsetCollection DoFPGrowthParallel(ItemsetCollection db, double supportThreshold)
        {


            ItemsetCollection Fre = new ItemsetCollection();//List of Items > support
            ItemsetCollection T = new ItemsetCollection();//frequent items
            List<ItemsetCollection> P = new List<ItemsetCollection>();
            ItemsetCollection FPTreeCon = new ItemsetCollection();//Conditional FP-Tree
            //item > support
            //Itemset I = db.GetUniqueItems();
            //foreach (string item in I)
            //{
            //    Itemset itemset = new Itemset() { item };
            //    itemset.Support = db.FindSupport(itemset);
            //    if (itemset.Support >= supportThreshold)
            //    {
            //        Fre.Add(itemset);
            //    }
            //}
            //// sort item
            //Fre.Sort(delegate (Itemset x, Itemset y)
            //{
            //    if (x.Support > y.Support) return -1;
            //    else if (x.Support < y.Support)
            //        return 1;
            //    else return 0;
            //});
            ////Fre = ParallelFre(db, supportThreshold);
            ////frequent items
            //foreach (Itemset itemdata in db)
            //{
            //    Itemset itemset = new Itemset();
            //    foreach (Itemset itemFre in Fre)
            //    {
            //        if (itemdata.Contains(itemFre[0]))
            //        {
            //            itemset.Add(itemFre[0]);
            //        }
            //    }
            //    T.Add(itemset);
            //}

            T = ParallelFPGrowth(db, supportThreshold, ref Fre);
            // create FP Tree
            FPTree tree = CreateTree(T);

            // Conditional Patern Bases
            foreach (Itemset itemset in Fre)
            {
                ItemsetCollection itemsetCollection = new ItemsetCollection();
                for (int j = 0; j < tree.countNode; j++)
                {
                    var node = tree.arrayNode[j];
                    if (node.itemName.Equals(itemset[0]) && !node.visited)
                    {
                        node.visited = true;
                        var nodeparent = node.nodeParent;
                        while (nodeparent.itemName > -1)
                        {
                            int index = itemsetCollection.FindIndex(nodeparent.itemName);
                            if (index > -1)
                            {
                                itemsetCollection[index].Support += node.count;
                            }
                            else
                            {
                                Itemset item = new Itemset();
                                item.Support = node.count;
                                item.Add(nodeparent.itemName);
                                itemsetCollection.Add(item);
                            }
                            nodeparent = nodeparent.nodeParent;
                        }
                    }
                }
                P.Add(itemsetCollection);
            }
            // Conditional FP-Tree
            for (int i = 0; i < P.Count; i++)
            {
                ItemsetCollection itemsetCollection = new ItemsetCollection();
                //itemsetCollection.Add(Fre[i]); //hoán vị sau
                foreach (var item in P[i])
                {
                    if (db.Support(item) >= supportThreshold)
                    {
                        itemsetCollection.Add(item);
                    }
                }
                ItemsetCollection subsets = Bit.FindSubsets(itemsetCollection.GetUniqueItems(), 0); //get all subsets
                foreach (Itemset items in subsets)
                {
                    items.Add(Fre[i][0]);
                    items.Support = db.FindSupport(items);
                    if (items.Support >= supportThreshold)
                        FPTreeCon.Add(items);
                }
            }
            return (FPTreeCon);
        }

        private static FPTree CreateTree(ItemsetCollection T)
        {
            FPTree tree = new FPTree();
            List[] list_frequencyItems_TID;

            int i = 0;
            list_frequencyItems_TID = ToList(T);
            for (i = 0; i < T.Count; i++)
            {
                List list = new List();
                list = list_frequencyItems_TID[i];
                tree = tree.InsertNode(tree, list);
            }
            return tree;
        }

        private static List[] ToList(ItemsetCollection frequencyItemsTID)
        {
            List[] mangList = new List[frequencyItemsTID.Count];
            int i = 0, j = 0;

            for (i = 0; i < frequencyItemsTID.Count; i++)
            {
                List list = new List();
                list = list.CreateList();
                Node node = new Node();
                //Tao List cho TID
                if(frequencyItemsTID[i]!=null)
                for (j = 0; j < frequencyItemsTID[i].Count; j++)
                {
                    node = node.CreateNode(frequencyItemsTID[i][j]);
                    list = list.InsertTail(list, node);
                }
                mangList[i] = list;
            }
            return mangList;
        }
    }
}
