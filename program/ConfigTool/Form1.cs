using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConfigTool
{
    public partial class Form1 : Form
    {
        private List<string> mFullPathList = new List<string>();

        private Task mTask;

        public Form1()
        {
            InitializeComponent();
            UpdateAllFileView();
        }

        ~Form1()
        {
            mTask?.Dispose();
        }

        public void UpdateAllFileView()
        {
            //保存之前的选中状态
            Dictionary<string, bool> itemsCheckedState = new Dictionary<string, bool>();
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                bool _checked = checkedListBox1.GetItemChecked(i);
                itemsCheckedState.Add(checkedListBox1.Items[i].ToString(), _checked);
            }

            //重新填充数据源
            PopulateCheckBoxList();

            //将之前选中的项重新选中
            foreach (var item in itemsCheckedState)
            {
                int index = checkedListBox1.FindStringExact(item.Key);
                if (index != -1)
                {
                    checkedListBox1.SetItemChecked(index, item.Value);
                }
            }
        }

        public void PopulateCheckBoxList()
        {
            if (!Directory.Exists(Define.SourceDirectory))
            {
                return;
            }

            mFullPathList.Clear();
            checkedListBox1.Items.Clear();
            List<string> sourceFilePaths = Utils.GetAllFilePaths(Define.SourceDirectory, Define.SourcePostfix);
            for (int i = 0; i < sourceFilePaths.Count; i++)
            {
                string path = sourceFilePaths[i];
                string name = Path.GetFileNameWithoutExtension(path);
                mFullPathList.Add(path);
                checkedListBox1.Items.Add(name);
                checkedListBox1.SetItemChecked(i, true); //默认选中
            }
        }

        public void Log(object content)
        {
            richTextBox1.SelectionColor = Color.Black;
            richTextBox1.AppendText(content.ToString() + "\n");
        }

        public void LogError(object content)
        {
            richTextBox1.SelectionColor = Color.Red;
            richTextBox1.AppendText(content.ToString() + "\n");
        }

        public void ClearLog()
        {
            richTextBox1.SelectionColor = Color.Black;
            richTextBox1.Text = "";
        }

        public void OnComplete()
        {
            Log("Done!");
            button1.Enabled = true;
            button2.Enabled = true;
            mTask.Dispose();
            mTask = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;

            List<string> selectedPathList = new List<string>();
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    selectedPathList.Add(mFullPathList[i]);
                }
            }

            mTask?.Dispose();
            mTask = new Task(() =>
            {
                Invoke(new Action(() =>
                {
                    ClearLog();
                    Log("Start Process!");
                }));
                FileProcess.Process(selectedPathList, mFullPathList, checkBox1.Checked);
            });
            mTask.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;

            mTask?.Dispose();
            mTask = new Task(() =>
            {
                Invoke(new Action(() =>
                {
                    ClearLog();
                    Log("Start Process!");
                }));
                FileProcess.Process(mFullPathList, mFullPathList, checkBox1.Checked);
            });
            mTask.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            UpdateAllFileView();
        }

        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            UpdateAllFileView();
        }
    }
}