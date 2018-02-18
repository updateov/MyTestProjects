using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SuperEnglish
{
    public partial class frmWordComp : Form
    {
        string[] words = {"bus","cat","dog","bag","car"};
        int currentWord = 0;
        string corrLetter;
        string answer;
        bool correct = false;
        bool doubleClicked = false;
        
        public frmWordComp()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private void pb_continue_Click(object sender, EventArgs e)
        {
            if (!doubleClicked)
            {
                if (answer.Equals(corrLetter))
                {
                    correct = true;
                    currentWord++;
                    pb_cloud.Image = Properties.Resources.cloud_good;
                    pb_smily.Image = Properties.Resources.smily_good;
                    doubleClicked = true;
                }
                else
                {
                    pb_cloud.Image = Properties.Resources.cloud_incorrect;
                    pb_smily.Image = Properties.Resources.smily_wrong;
                } 
            }

            //LoadNextWord();
        }

        private void pb_letter_Click(object sender, EventArgs e)
        {
            string file = ((PictureBox)sender).Tag.ToString();
            answer = file;
            file += file;
            object myObj = Properties.Resources.ResourceManager.GetObject(file);
            //((PictureBox)sender).Image = (Image)myObj;
            pb_word_2.Image = (Image)myObj;
        }

        private void frmWordComp_Shown(object sender, EventArgs e)
        {
            LoadNextWord();
        }

        private void LoadNextWord()
        {
            pb_word_1.Image = (Image)(Properties.Resources.ResourceManager.GetObject((words[currentWord])[0].ToString().ToUpper()));
            pb_word_3.Image = (Image)(Properties.Resources.ResourceManager.GetObject((words[currentWord])[2].ToString().ToUpper()));
            pb_word_2.Image = Properties.Resources.space;
            corrLetter=(words[currentWord])[1].ToString().ToUpper();            
        }
    }
}
