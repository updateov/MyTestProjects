using System.Collections.Generic;
using System.Windows.Forms;

namespace ProfileResultViewer
{
    public static class LineListExtensions
    {
        /// <summary>
        /// Sorts a list of Line-items by the Inclusive-Field
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<Line> SortedAccess(this List<Line> list)
        {
            list.Sort((l1, l2) => { return (int)(l2.Inclusive * 100 - l1.Inclusive * 100); });
            foreach (var item in list)
            {
                yield return item;
            }
        }
    }

    public static class TreeNodeExtensions
    {
        /// <summary>
        /// Returns true if node contains a subnode with the specified name
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Contains(this TreeNode node, string name)
        {
            if (node != null)
            {
                return node.Name == name || node.Parent.Contains(name);
            }
            return false;
        }
    }
}
