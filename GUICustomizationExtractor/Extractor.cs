using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GUICustomizationExtractor
{
    public class Extractor
    {
        List<String> CSharpFile { get; set; }

        public Extractor()
        {
        }

        public bool Process(String sourceFile, String destinationFile, String dialogName, Object dialogContent)
        {
            try
            {
                String readText = File.ReadAllText(sourceFile);
                CSharpFile = readText.Split('\n').ToList();

                var autoScaleSize = (from c in CSharpFile
                                     where c.IndexOf("AutoScaleBaseSize") > -1
                                     select c).First().Trim().Replace(";", "");

                var autScaleWidthStr = autoScaleSize.Remove(0, autoScaleSize.IndexOf("(") + 1);
                autScaleWidthStr = autScaleWidthStr.Remove(autScaleWidthStr.IndexOf(","));
                int autoScaleWidth;
                bool bAuto = Int32.TryParse(autScaleWidthStr, out autoScaleWidth);
                var clientSize = (from c in CSharpFile
                                  where c.IndexOf("ClientSize") > -1
                                  select c).First().Trim().Replace(";", "");

                var clientWidthStr = clientSize.Remove(0, clientSize.IndexOf("(") + 1);
                clientWidthStr = clientWidthStr.Remove(clientWidthStr.IndexOf(","));
                int clientWidth;
                bool bClient = Int32.TryParse(clientWidthStr, out clientWidth);
                int width = (bAuto ? autoScaleWidth : 0) + (bClient ? clientWidth : 0);
                String content = dialogContent.ToString().Replace(" ", "");
                XElement dialog = new XElement("Control",
                                        new XAttribute("Class", "LMS.GUI." + content),
                                        new XAttribute("Name", content),
                                        new XAttribute("Width", width.ToString()));

                XElement toSave = new XElement("Root", new XElement(content, dialog));

                var dialogControls = (from c in CSharpFile
                                      where c.IndexOf("this.Controls.Add(") > -1
                                      select c.Trim().Replace(";", "")).ToList();

                FillControls(dialogControls, dialog);
                toSave.Save(destinationFile);
                MessageBox.Show("File extracted successfully");
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void FillControls(List<String> controls, XElement parent)
        {
            foreach (var item in controls)
            {
                String controlName = item.Remove(0, item.IndexOf("("));
                controlName = controlName.Remove(0, controlName.IndexOf("m_"));
                controlName = controlName.Remove(controlName.IndexOf(")"));
                var location = (from c in CSharpFile
                                where c.IndexOf(controlName + ".Location") > -1
                                select c.Trim().Replace(";", "")).FirstOrDefault();

                var size = (from c in CSharpFile
                            where c.IndexOf(controlName + ".Size") > -1
                            select c.Trim().Replace(";", "")).FirstOrDefault();

                var controlType = GetControlType(controlName);
                if (location != null && !location.Equals(String.Empty) &&
                    size != null && !size.Equals(String.Empty) &&
                    !controlType.Equals(String.Empty))
                {
                    var bounds = GetBounds(location, size);
                    var curElem = new XElement("Control",
                                        new XAttribute("Class", controlType),
                                        new XAttribute("Name", controlName),
                                        new XAttribute("Bounds", bounds));

                    var addedControls = (from c in CSharpFile
                                         where c.IndexOf(controlName + ".Controls.Add(") > -1
                                         select c.Trim().Replace(";", "")).ToList();

                    if (addedControls.Count > 0) // Recursion
                        FillControls(addedControls, curElem);

                    parent.Add(curElem);
                }
            }
        }

        private String GetBounds(String location, String size)
        {
            String toRet = String.Empty;
            var tmpStr = location.Remove(0, location.IndexOf("("));
            toRet = tmpStr.Replace("(", "");
            toRet = toRet.Remove(toRet.IndexOf(")")) + ", ";
            tmpStr = size.Remove(0, size.IndexOf("(")).Replace("(", "");
            tmpStr = tmpStr.Remove(tmpStr.IndexOf(")"));
            toRet += tmpStr;
            return toRet;
        }

        private String GetControlType(String controlName)
        {
            var controlDef = (from c in CSharpFile
                              where c.IndexOf(controlName + ";") > 0
                              select c.Trim()).ToList().Where(d => d.IndexOf("private") > -1).FirstOrDefault();

            if (controlDef == null || controlDef.Equals(String.Empty))
                return String.Empty;

            String toRet = controlDef.Remove(controlDef.IndexOf(controlName)).Replace("private", "").Trim();
            return toRet;
        }

        private void FillGroups(String tabName)
        {
            var controls = (from c in CSharpFile
                              where c.IndexOf(tabName + ".Controls.Add(") > -1
                              select c.Trim().Replace(";", "")).ToList();

            var groupBoxes = (from c in controls
                              where c.IndexOf("m_grp") > -1
                              select c).ToList();
        }
    }
}
