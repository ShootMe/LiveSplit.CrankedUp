﻿using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Model;
namespace LiveSplit.CrankedUp {
    public partial class UserSettings : UserControl {
        public SplitterSettings Settings { get; set; }
        private LiveSplitState State;
        private LogManager Log;

        public UserSettings(LiveSplitState state, LogManager log) {
            InitializeComponent();
            Settings = new SplitterSettings();
            State = state;
            Log = log;
            Dock = DockStyle.Fill;
        }

        public void AddXmlItem<T>(XmlDocument document, XmlElement xmlSettings, string name, T value) {
            XmlElement xmlItem = document.CreateElement(name);
            xmlItem.InnerText = value.ToString();
            xmlSettings.AppendChild(xmlItem);
        }
        public bool GetXmlBoolItem(XmlNode node, string path, bool defaultValue) {
            XmlNode item = node.SelectSingleNode(path);
            bool value = defaultValue;
            if (item != null) {
                bool.TryParse(item.InnerText, out value);
            }
            return value;
        }
        public XmlNode UpdateSettings(XmlDocument document) {
            XmlElement xmlSettings = document.CreateElement("Settings");

            AddXmlItem<bool>(document, xmlSettings, "LogInfo", chkLog.Checked);
            Log.EnableLogging = chkLog.Checked;

            XmlElement xmlSplits = document.CreateElement("Splits");
            xmlSettings.AppendChild(xmlSplits);

            foreach (Split split in Settings.Autosplits) {
                XmlElement xmlSplit = document.CreateElement("Split");
                xmlSplit.InnerText = split.ToString();

                xmlSplits.AppendChild(xmlSplit);
            }

            return xmlSettings;
        }
        public void InitializeSettings(XmlNode node) {
            Settings.Autosplits.Clear();

            bool logInfo = GetXmlBoolItem(node, ".//LogInfo", false);
            chkLog.Checked = logInfo;
            Log.EnableLogging = logInfo;

            XmlNodeList splitNodes = node.SelectNodes(".//Splits/Split");
            foreach (XmlNode splitNode in splitNodes) {
                string[] splitValues = splitNode.InnerText.Split('|');
                if (splitValues.Length == 2) {
                    SplitType type = SplitType.ManualSplit;
                    if (Enum.TryParse<SplitType>(splitValues[0], out type)) {
                        string value = splitValues[1];
                        Settings.Autosplits.Add(new Split() { Type = type, Value = value });
                    }
                }
            }

            FixSplits();
        }
        private void Settings_Load(object sender, EventArgs e) {
            Form form = FindForm();
            form.Text = "Cranked Up v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

            FixSplits();
        }
        private void FixSplits() {
            int index = 1;
            bool changed = false;
            if (Settings.Autosplits.Count == 0) {
                Settings.Autosplits.Add(new Split() { Name = "Auto Start", Type = SplitType.GameStart });
                changed = true;
            } else {
                Settings.Autosplits[0].Name = "Auto Start";
            }

            foreach (ISegment segment in State.Run) {
                if (index < Settings.Autosplits.Count) {
                    Split split = Settings.Autosplits[index++];
                    if (split.Name != segment.Name) {
                        split.Name = segment.Name;
                        changed = true;
                    }
                } else {
                    index++;
                    Settings.Autosplits.Add(new Split() { Name = segment.Name, Type = SplitType.Level, Value = "Any" });
                    changed = true;
                }
            }

            while (index < Settings.Autosplits.Count) {
                Settings.Autosplits.RemoveAt(Settings.Autosplits.Count - 1);
                changed = true;
            }

            if (changed) {
                flowMain.SuspendLayout();
                flowMain.Controls.Clear();

                foreach (Split split in Settings.Autosplits) {
                    UserSplitSettings setting = new UserSplitSettings();
                    setting.UserSplit = split;
                    setting.UpdateControls(true);
                    flowMain.Controls.Add(setting);
                }

                flowMain.ResumeLayout(true);
            }
        }
        private void btnLog_Click(object sender, EventArgs e) {
            DataTable dt = new DataTable();
            dt.Columns.Add("Event", typeof(string));
            try {
                if (File.Exists(LogManager.LOG_FILE)) {
                    using (StreamReader sr = new StreamReader(LogManager.LOG_FILE)) {
                        string line;
                        while (!string.IsNullOrEmpty(line = sr.ReadLine())) {
                            dt.Rows.Add(line);
                        }
                    }
                }

                using (LogViewer logViewer = new LogViewer() { DataSource = dt }) {
                    logViewer.ShowDialog(this);
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnClearLog_Click(object sender, EventArgs e) {
            Log.Clear(true);
            MessageBox.Show(this, "Debug Log has been cleared.", "Debug Log", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
        private void flowMain_DragEnter(object sender, DragEventArgs e) {
            e.Effect = DragDropEffects.Move;
        }
        private void flowMain_DragOver(object sender, DragEventArgs e) {
            UserSplitSettings oldItem = (UserSplitSettings)e.Data.GetData(typeof(UserSplitSettings));
            FlowLayoutPanel destination = (FlowLayoutPanel)sender;
            Point point = destination.PointToClient(new Point(e.X, e.Y));
            UserSplitSettings newItem = destination.GetChildAtPoint(point) as UserSplitSettings;
            int newIndex = destination.Controls.GetChildIndex(newItem, false);
            e.Effect = DragDropEffects.Move;
            int oldIndex = destination.Controls.GetChildIndex(oldItem);
            if (oldIndex != newIndex) {
                string segment = oldItem.UserSplit.Name;
                oldItem.UserSplit.Name = newItem.UserSplit.Name;
                newItem.UserSplit.Name = segment;
                Split split = Settings.Autosplits[oldIndex];
                Settings.Autosplits[oldIndex] = Settings.Autosplits[newIndex];
                Settings.Autosplits[newIndex] = split;
                oldItem.UpdateControls(false, false);
                newItem.UpdateControls(false, false);
                destination.Controls.SetChildIndex(oldItem, newIndex);
                destination.Invalidate();
            }
        }
    }
}