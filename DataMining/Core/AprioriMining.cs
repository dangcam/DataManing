using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMining.Core
{
    public class AprioriMining
    {
        public static ItemsetCollection DoApriori(ItemsetCollection db, double supportThreshold)
        {
            Itemset I = db.GetUniqueItems();
            ItemsetCollection L = new ItemsetCollection(); //resultant large itemsets
            ItemsetCollection Li = new ItemsetCollection(); //large itemset in each iteration
            ItemsetCollection Ci = new ItemsetCollection(); //pruned itemset in each iteration

            //first iteration (1-item itemsets)
            foreach (int item in I)
            {
                Ci.Add(new Itemset() { item });
            }

            //next iterations
            int k = 2;
            while (Ci.Count != 0)
            {
                //set Li from Ci (pruning)
                Li.Clear();
                foreach (Itemset itemset in Ci)
                {
                    itemset.Support = db.FindSupport(itemset);
                    if (itemset.Support >= supportThreshold)
                    {
                        Li.Add(itemset);
                        L.Add(itemset);
                    }
                }

                //set Ci for next iteration (find supersets of Li)
                Ci.Clear();
                Ci.AddRange(Bit.FindSubsets(Li.GetUniqueItems(), k)); //get k-item subsets
                k += 1;
            }

            return (L);
        }
        public static ItemsetCollection DoAprioriNew(ItemsetCollection db, double supportThreshold)
        {
            //Itemset I = db.GetUniqueItems();
            ItemsetCollection Fre = new ItemsetCollection();//List of Items > support
            ItemsetCollection L = new ItemsetCollection(); //resultant large itemsets
            ItemsetCollection T = new ItemsetCollection();//frequent items

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
            ItemsetCollection subsets = Bit.FindSubsets(Fre.GetUniqueItems(), 0); //get all subsets

            //Dictionary<Itemset, int> subsetsDic = subsets.Select((x, i) => new {x = x, i = 0 }).ToDictionary(k => k.x,k=>k.i);

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
                foreach (Itemset items in subsets)
                {
                    if (itemset.Contains(items))
                    {
                        items.Support++;
                    }
                }
            }

            foreach (Itemset items in subsets)
            {
                if (items.Count > 0)
                {
                    items.Support = (items.Support/(double)T.Count) * 100.0;
                    if (items.Support >= supportThreshold)
                        L.Add(items);
                }
            }
            return (L);
        }
        public static List<AssociationRule> Mine(ItemsetCollection db, ItemsetCollection L, double confidenceThreshold)
        {
            List<AssociationRule> allRules = new List<AssociationRule>();

            foreach (Itemset itemset in L)
            {
                ItemsetCollection subsets = Bit.FindSubsets(itemset, 0); //get all subsets
                foreach (Itemset subset in subsets)
                {
                    double confidence = (itemset.Support / db.FindSupport(subset)) * 100.0;
                    if (confidence >= confidenceThreshold)
                    {
                        AssociationRule rule = new AssociationRule();
                        rule.X.AddRange(subset);
                        rule.Y.AddRange(itemset.Remove(subset));
                        rule.Support = db.FindSupport(itemset);
                        rule.Confidence = confidence;
                        if (rule.X.Count > 0 && rule.Y.Count > 0)
                        {
                            allRules.Add(rule);
                        }
                    }
                }
            }

            return (allRules);
        }
        private double FindSupport(ItemsetCollection L, Itemset subset)
        {
            double min = 100;
            foreach (Itemset itemset in L)
            {
                if (itemset.Contains(subset))
                    if (min > itemset.Support)
                        min = itemset.Support;
            }
            return min;
        }
    }
}
