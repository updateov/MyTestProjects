using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ProfileResultViewer
{
    public partial class Viewer : Form
    {
        private const short MaxDepth = 10; // Maximum number of children of a node
        private const short MaxMainNodeCount = 100; // Maximum number of nodes in topmost hierarchy

        public LineReader Reader { get; private set; }
        public Viewer()
        {
            InitializeComponent();
        }

        private void Viewer_Shown(object sender, EventArgs args)
        {
            OpenFile();
        }

        private void OpenFile()
        {
            OpenFileDialog dlg = new OpenFileDialog(); ;
            dlg.FileName = @"./CallerCalleeSummary.txt";
            var result = dlg.ShowDialog();
            if (result != DialogResult.OK)
                return;

            try
            {
                Reader = new LineReader();
                Reader.ReadLines(dlg.FileName);
                UpdateView();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error processing file: " + e.Message);
            }
        }

        private void UpdateView()
        {
            if (Reader == null)
                return;

            IEnumerable<Line> list;
            if (radioBtnCallees.Checked)
                list = Reader.Callees();
            else if (radioBtnCallers.Checked)
                list = Reader.Callers();
            else
                list = Reader.Roots();

            this.SuspendLayout();
            treeView.SuspendLayout();

            treeView.Nodes.Clear();
            foreach (var item in list)
            {
                FillTree(item);

                if (treeView.Nodes.Count >= MaxMainNodeCount)
                {
                    treeView.Nodes.Add(new TreeNode(String.Format(
                        "Truncated (showing {0} of {1} items)",
                        treeView.Nodes.Count,
                        list.Count())));
                    break;
                }
            }

            treeView.ResumeLayout();
            this.ResumeLayout();
        }

        private void FillTree(Line line)
        {
            TreeNode node = GetNodeWithDummyChild(line);
            treeView.Nodes.Add(node);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void radioBtn_CheckedChanged(object sender, EventArgs e)
        {
            UpdateView();
        }

        private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            // check if expansion already has been computed
            if (e.Node.Nodes.Count == 0 || e.Node.Nodes.Count > 1)
                return;

            if (e.Node.Nodes[0].Text != "")
                return;

            e.Node.Nodes.Clear(); // remove dummy childnode

            var line = e.Node.Tag as Line;
            if (line == null)
                return;

            foreach (var childnode in line.Callees)
            {
                e.Node.Nodes.Add(GetNodeWithDummyChild(childnode));
            }
        }

        private TreeNode GetNodeWithDummyChild(Line line)
        {
            var result = new TreeNode(line.ToString());
            result.Tag = line;
            result.Nodes.Add(new TreeNode("")); // dummy for displaying "+"-sign
            return result;
        }
    }
}
