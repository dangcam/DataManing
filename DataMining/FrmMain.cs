using DataMining.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataMining
{
    public partial class FrmMain : Form
    {
        ItemsetCollection db;
        public FrmMain()
        {
            InitializeComponent();
        }
        private string file = "";
        private void FrmMain_Load(object sender, EventArgs e)
        {
            ResetData();
        }
        private void ResetData()
        {
            //sample items
            Itemset items = new Itemset();
            items.Add(0);
            items.Add(1);
            items.Add(2);
            items.Add(3);
            items.Add(4);
            items.Add(5);
            items.Add(6);
            items.Add(7);
            items.Add(8);
            txtInputItem.Text = string.Join(",", items.ToArray());

            //sample database
            db = new ItemsetCollection();
            db.Add(new Itemset() { items[0], items[1], items[2], items[3], items[4] });
            db.Add(new Itemset() { items[1], items[2] });
            db.Add(new Itemset() { items[0], items[1], items[5] });
            db.Add(new Itemset() { items[1], items[0], items[6] });
            db.Add(new Itemset() { items[0], items[5], items[7] });
            richItems.Text = db.ToString();
            txtTransaction.Text = db.Count.ToString();

            txtSupportThreshold.Text = "40";
            txtConfidenceThreshold.Text = "70";
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            ResetData();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            //get items specified by user
            Itemset items = new Itemset();
            //items.AddRange(txtInputItem.Text.Split(','));
            foreach (string it in txtInputItem.Text.Split(','))
            {
                items.Add(int.Parse(it));
            }
            //items.Remove("");
            int transactionCount = 5;
            int.TryParse(txtTransaction.Text, out transactionCount);
            Random rnd = new Random();
            //create random transactions
            db = new ItemsetCollection();
            for (int transactionIndex = 0; transactionIndex < transactionCount;)
            {
                int itemCount = rnd.Next(2, items.Count);

                Itemset transaction = new Itemset();
                //for (int itemIndex = 0; itemIndex < itemCount; itemIndex++)
                //{
                //    int randomItemIndex = rnd.Next(items.Count);
                //    if (!transaction.Contains(items[randomItemIndex]))
                //    {
                //        transaction.Add(items[randomItemIndex]);
                //    }
                //}
                while (itemCount > 0)
                {
                    int randomItemIndex = rnd.Next(items.Count);
                    if (!transaction.Contains(items[randomItemIndex]))
                    {
                        transaction.Add(items[randomItemIndex]);
                        itemCount--;
                    }
                }

                if (transaction.Count > 0)
                {
                    db.Add(transaction);
                    transactionIndex += 1;
                }
            }

            richItems.Text = db.ToString();
        }

        private void btnApriori_Click(object sender, EventArgs e)
        {
            richAssociationApriori.Text = string.Empty;
            richLargeApriori.Text = string.Empty;
            //do apriori
            double supportThreshold = double.Parse(txtSupportThreshold.Text);
            System.Diagnostics.Stopwatch calcTime = System.Diagnostics.Stopwatch.StartNew();
            ItemsetCollection L = AprioriMining.DoApriori(db, supportThreshold);
            richLargeApriori.Text = (L.Count + " Large Itemsets (by Apriori)") + "\r\n";
            txtTimesApriori.Text = ((calcTime.ElapsedMilliseconds)) + " Milliseconds";
            foreach (Itemset itemset in L)
            {
                richLargeApriori.Text += itemset.ToString() + "\r\n";
            }

            //do mining
            double confidenceThreshold = double.Parse(txtConfidenceThreshold.Text);

            List<AssociationRule> allRules = AprioriMining.Mine(db, L, confidenceThreshold);
            richAssociationApriori.Text = (allRules.Count + " Association Rules \n");
            foreach (AssociationRule rule in allRules)
            {
                richAssociationApriori.Text += rule.ToString() + "\r\n";
            }

        }

        private void btnFPGrowth_Click(object sender, EventArgs e)
        {
            richAssociationFPGrowth.Text = string.Empty;
            richLargeFPGrowth.Text = string.Empty;
            // do FP-Growth
            double supportThreshold = double.Parse(txtSupportThreshold.Text);
            System.Diagnostics.Stopwatch calcTime = System.Diagnostics.Stopwatch.StartNew();
            ReturnL();//FPGrowthMining.DoFPGrowthParallel(db, supportThreshold);
            txtTimesFPGrowth.Text = ((calcTime.ElapsedMilliseconds)) + " Milliseconds";

            //
            string[] database = null;
            try { database = System.IO.File.ReadAllLines("OutputFPGrowth.txt"); }
            catch { }
            ItemsetCollection L = new ItemsetCollection();
            Itemset items;
            foreach (string item in database)
            {
                items = new Itemset();
                string[] itemsupport = item.Split(':');
                //items.AddRange(itemsupport[0].Split(','));
                foreach (string it in itemsupport[0].Split(','))
                {
                    items.Add(int.Parse(it));
                }
                //items.Remove("");
                items.Support = double.Parse(itemsupport[1]);
                L.Add(items);
            }
            //

            richLargeFPGrowth.Text = (L.Count + " Large Itemsets (by FPGrowth)") + "\r\n";
            foreach (Itemset itemset in L)
            {
                richLargeFPGrowth.Text += itemset.ToString() + "\r\n";
            }

            //do mining
            double confidenceThreshold = double.Parse(txtConfidenceThreshold.Text);

            List<AssociationRule> allRules = AprioriMining.Mine(db, L, confidenceThreshold);
            richAssociationFPGrowth.Text = (allRules.Count + " Association Rules \n");
            foreach (AssociationRule rule in allRules)
            {
                richAssociationFPGrowth.Text += rule.ToString() + "\r\n";
            }

        }
        private void ReturnL()
        {
            //ItemsetCollection L = new ItemsetCollection();
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "MPIEXEC";
            startInfo.Arguments = "-n "+txtTienTrinh.Text+" Mpi.NET1.exe \""+file+"\" \""+ txtSupportThreshold.Text+"\" ";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
           
            //return L;
        }
        private void ReturnLApriori()
        {
            //ItemsetCollection L = new ItemsetCollection();
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "MPIEXEC";
            startInfo.Arguments = "-n " + txtTienTrinhApriori.Text + " Mpi.NET2.exe \"" + file + "\" \"" + txtSupportThreshold.Text + "\" ";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            //return L;
        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "E:\\";
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                int i = 0;
                try
                {
                    if ((openFileDialog.OpenFile()) != null)
                    {
                        file = openFileDialog.FileName;
                        string[] database = System.IO.File.ReadAllLines(file);
                        db = new ItemsetCollection();
                        Itemset items;
                        
                        foreach(string item in database)
                        {
                            items = new Itemset();
                            //items.AddRange(item.Split(','));
                            //items.Remove("");
                            foreach (string it in item.Split(','))
                            {
                                items.Add(int.Parse(it));
                            }
                            db.Add(items);
                            i++;
                        }
                        
                        richItems.Text = db.ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message+ " "+i);
                }
            }
        }

        private void btnFPG_Click(object sender, EventArgs e)
        {
            richAssociationFPG.Text = string.Empty;
            richLargeFPG.Text = string.Empty;
            // do FP-Growth
            double supportThreshold = double.Parse(txtSupportThreshold.Text);
            System.Diagnostics.Stopwatch calcTime = System.Diagnostics.Stopwatch.StartNew();
            ItemsetCollection L = FPGrowthMining.DoFPGrowth(db, supportThreshold);
            txtTimesFPG.Text = ((calcTime.ElapsedMilliseconds)) + " Milliseconds";
            richLargeFPG.Text = (L.Count + " Large Itemsets (by FPGrowth)") + "\r\n";
            foreach (Itemset itemset in L)
            {
                richLargeFPG.Text += itemset.ToString() + "\r\n";
            }

            //do mining
            double confidenceThreshold = double.Parse(txtConfidenceThreshold.Text);

            List<AssociationRule> allRules = AprioriMining.Mine(db, L, confidenceThreshold);
            richAssociationFPG.Text = (allRules.Count + " Association Rules \n");
            foreach (AssociationRule rule in allRules)
            {
                richAssociationFPG.Text += rule.ToString() + "\r\n";
            }
        }

        private void btnAprioriNew_Click(object sender, EventArgs e)
        {
            richAssociationAprioriNew.Text = string.Empty;
            richLargeAprioriNew.Text = string.Empty;
            //do apriori
            double supportThreshold = double.Parse(txtSupportThreshold.Text);
            System.Diagnostics.Stopwatch calcTime = System.Diagnostics.Stopwatch.StartNew();
            ItemsetCollection L = AprioriMining.DoAprioriNew(db, supportThreshold);
            richLargeAprioriNew.Text = (L.Count + " Large Itemsets (by Apriori)") + "\r\n";
            txtTimesAprioriNew.Text = ((calcTime.ElapsedMilliseconds)) + " Milliseconds";
            foreach (Itemset itemset in L)
            {
                richLargeAprioriNew.Text += itemset.ToString() + "\r\n";
            }

            //do mining
            double confidenceThreshold = double.Parse(txtConfidenceThreshold.Text);

            List<AssociationRule> allRules = AprioriMining.Mine(db, L, confidenceThreshold);
            richAssociationAprioriNew.Text = (allRules.Count + " Association Rules \n");
            foreach (AssociationRule rule in allRules)
            {
                richAssociationAprioriNew.Text += rule.ToString() + "\r\n";
            }
        }

        private void btnAprioriNewSS_Click(object sender, EventArgs e)
        {
            richAssociationAprioriNewSS.Text = string.Empty;
            richLargeAprioriNewSS.Text = string.Empty;
            // do FP-Growth
            double supportThreshold = double.Parse(txtSupportThreshold.Text);
            System.Diagnostics.Stopwatch calcTime = System.Diagnostics.Stopwatch.StartNew();
            ReturnLApriori();//FPGrowthMining.DoFPGrowthParallel(db, supportThreshold);
            txtTimesAprioriNewSS.Text = ((calcTime.ElapsedMilliseconds)) + " Milliseconds";

            //
            string[] database = null;
            try { database = System.IO.File.ReadAllLines("OutputAprioriNew.txt"); }
            catch { }
            ItemsetCollection L = new ItemsetCollection();
            Itemset items;
            foreach (string item in database)
            {
                items = new Itemset();
                string[] itemsupport = item.Split(':');
                //items.AddRange(itemsupport[0].Split(','));
                foreach (string it in itemsupport[0].Split(','))
                {
                    items.Add(int.Parse(it));
                }
                //items.Remove("");
                items.Support = double.Parse(itemsupport[1]);
                L.Add(items);
            }
            //

            richLargeAprioriNewSS.Text = (L.Count + " Large Itemsets (by Apriori cải tiến song song)") + "\r\n";
            foreach (Itemset itemset in L)
            {
                richLargeAprioriNewSS.Text += itemset.ToString() + "\r\n";
            }

            //do mining
            double confidenceThreshold = double.Parse(txtConfidenceThreshold.Text);

            List<AssociationRule> allRules = AprioriMining.Mine(db, L, confidenceThreshold);
            richAssociationAprioriNewSS.Text = (allRules.Count + " Association Rules \n");
            foreach (AssociationRule rule in allRules)
            {
                richAssociationAprioriNewSS.Text += rule.ToString() + "\r\n";
            }
        }
    }
}
