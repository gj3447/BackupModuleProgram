using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using BackupModuleSystem;
using System.Threading.Tasks;
using System.Threading;
using DevExpress.XtraBars;
using System.ServiceModel.Channels;

namespace BackupModuleTest
{
    public partial class Form1 : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public BackupModuleSystem.BackupModuleSystem m_backup_module_system;
        public string m_backup_path_create_from;
        public string m_backup_path_create_to;
        public bool m_threading;
        private CancellationTokenSource _cancellationTokenSource;

        public List<BackupModuleSystem.BackupModuleSystemT> m_db_all_load;
        public BackupModuleSystem.BackupModuleSystemT m_db_selected;
        public string m_db_name;
        public Form1()
        {
            InitializeComponent();
            m_backup_module_system = new BackupModuleSystem.BackupModuleSystem();
            BackupToSelectedUpdate();
            BackupToListUpdate();
            BackupFromListUpdate();
            BackupFromSelectedUpdate();
            DataBaseUpdate();
            m_threading = false;
            StartAutoUpdate();
            Console.WriteLine("시작");
            timer.Text = "time : " + DateTime.Now.ToString();
        }
        private void StartAutoUpdate()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        // 핸들 생성될 때까지 대기
                        while (!this.IsHandleCreated)
                        {
                            await Task.Delay(100);
                        }
                        while (m_threading)
                        {
                            Task.Delay(100);
                        }
                        m_threading = true;
                        // UI 업데이트는 반드시 Invoke 필요
                        this.Invoke((MethodInvoker)delegate
                        {
                            List<string> string_list = new List<string>();
                            foreach (BackupFrom e in m_backup_module_system.m_backup_from_list)
                            {
                                e.h_update(out List<string> message_list);
                                foreach (string a in message_list)
                                {
                                    if (a != null && a != "")
                                    {
                                        string_list.Add(a);
                                    }
                                }
                            }
                            if (string_list.Count > 0)
                            {
                                string alram = "";
                                foreach (string e in string_list)
                                {
                                    alram += e;
                                }
                                if(Func.is_success(alram))
                                {
                                    MessageBox.Show("예약된 백업 작업이 실행되었습니다");
                                }
                                else
                                {
                                    MessageBox.Show(alram);
                                }
                                BackupToListUpdate();
                                BackupToSelectedUpdate();
                                BackupFromSelectedUpdate();
                            }
                            timer.Text = "time : " + DateTime.Now.ToString();
                        });
                        m_threading = false;
                        await Task.Delay(TimeSpan.FromSeconds(1), _cancellationTokenSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        Console.WriteLine("쓰레드 내부 오류");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"예외 발생: {ex.Message}");
                    }
                }
            }, _cancellationTokenSource.Token);
        }
        private void threading_alarm()
        {
            //MessageBox.Show("업데이트 함수 동작 중입니다. 잠시 후에 조작해주세요");
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            base.OnFormClosing(e);
        }
        static bool IsPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                // 경로에 포함될 수 없는 문자가 있는지 확인
                return path.IndexOfAny(Path.GetInvalidPathChars()) == -1;
            }
            catch
            {
                return false;
            }
        }
        private void bbtnBackupToPath_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            string path;
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "폴더를 선택하세요.";
                folderDialog.ShowNewFolderButton = true; // 새 폴더 만들기 버튼 표시

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    path = folderDialog.SelectedPath;
                    m_backup_path_create_to = path;
                    lblBackupToPath.Caption = path;
                    beiBackupToPath.EditValue = path;
                }
            }
            m_threading = false;
        }

        private void bbtnBackupFromPath_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            string path;
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "폴더를 선택하세요.";
                folderDialog.ShowNewFolderButton = true; // 새 폴더 만들기 버튼 표시

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    path = folderDialog.SelectedPath;
                    m_backup_path_create_from = path;
                    lblBackupFromPath.Caption = path;
                    beiBackupFromPath.EditValue = path;
                }
            }
            m_threading = false;
        }

        private void beiBackupFromPath_EditValueChanged(object sender, EventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            string path = beiBackupFromPath.EditValue?.ToString();
            if (path != null)
            {
                if (IsPath(path))
                {
                    m_backup_path_create_from = path;
                    lblBackupFromPath.Caption = path;
                }
            }
            m_threading = false;
        }

        private void beiBackupToPath_EditValueChanged(object sender, EventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            string path = beiBackupToPath.EditValue?.ToString();
            if (path != null)
            {
                if (IsPath(path))
                {
                    m_backup_path_create_to = path;
                    lblBackupToPath.Caption = path;
                }
            }
            m_threading = false;
        }

        private void bbtnBackupPathCreateCreate_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            BackupFrom result =
            m_backup_module_system.h_add_backup_from
            (m_backup_path_create_from, m_backup_path_create_to, out string message);
            if (result != null)
            {
                BackupFromListUpdate();
                BackupFromSelectedUpdate();
                BackupToListUpdate();
            }
            if (Func.is_success(message))
            {
                MessageBox.Show("백업 경로가 생성되었습니다.");
            }
            else
            {
                MessageBox.Show(message);
            }

            m_threading = false;
        }
        private void BackupFromListUpdate()
        {
            lstbBackupFrom.DataSource = m_backup_module_system.m_backup_from_list;
        }
        private void BackupToListUpdate()
        {
            if (m_backup_module_system.m_backup_from_selected == null) return;
            lstbBackupTo.DataSource =
                m_backup_module_system.m_backup_from_selected.m_backup_to_list;
        }
        private void BackupToSelectedUpdate()
        {
            if (m_backup_module_system.m_backup_to_selected == null)
            {
                rpgBackupToAction.Visible = false;
                rpgBackupToSelected.Visible = false;
            }
            else
            {
                lblBackupToSelected.Caption = m_backup_module_system.m_backup_to_selected.ToString();
                rpgBackupToAction.Visible = true;
                rpgBackupToSelected.Visible = true;
            }

        }
        private void BackupFromSelectedUpdate()
        {
            if (m_backup_module_system.m_backup_from_selected == null)
            {
                rpgBackupFromSelected.Visible = false;
                rpgBackupFromAction.Visible = false;
                rpgBackupFromSchedule.Visible = false;
                rpgBackupFromWeek.Visible = false;
            }
            else
            {
                BackupFrom backup_from = m_backup_module_system.m_backup_from_selected;

                lstbBackupTo.DataSource = backup_from.m_backup_to_list;

                rpgBackupFromSelected.Visible = true;
                rpgBackupFromAction.Visible = true;
                rpgBackupFromSchedule.Visible = true;
                rpgBackupFromWeek.Visible = true;

                ckbMon.EditValue = backup_from.m_mon;
                ckbTue.EditValue = backup_from.m_tue;
                ckbWed.EditValue = backup_from.m_wed;
                ckbThu.EditValue = backup_from.m_thu;
                ckbFri.EditValue = backup_from.m_fri;
                ckbSat.EditValue = backup_from.m_sat;
                ckbSun.EditValue = backup_from.m_sun;
                ckbRun.EditValue = backup_from.m_run;

                timeEditBackupFromWeek.EditValue = backup_from.m_week_update_time;
                lblBackupFromSelected.Caption = backup_from.ToString();

                TimeOnly timeOnly = m_backup_module_system.m_backup_from_selected.m_week_update_time;
                DateTime dateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day,
                                                 timeOnly.Hour, timeOnly.Minute, timeOnly.Second);

                timeEditBackupFromWeek.EditValue = dateTime;
                listBackupFromSchedule.Strings.Clear();
                foreach (DateTime e in backup_from.m_schedule_update_list)
                {
                    listBackupFromSchedule.Strings.Add(e.ToString());
                }

            }
        }
        private void DataBaseUpdate()
        {
            listBackupSettingSettingList.Strings.Clear();
            m_db_all_load = BackupModuleSystem.BackupModuleSystem.h_db_load_all(out string message);
            foreach (BackupModuleSystemT e in m_db_all_load)
            {
                listBackupSettingSettingList.Strings.Add(e.ToString());
            }
        }
        private void timeEditBackupFromWeek_EditValueChanged(object sender, EventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            if (timeEditBackupFromWeek.EditValue is DateTime)
            {
                if (timeEditBackupFromWeek.EditValue != null && m_backup_module_system.m_backup_from_selected != null)
                {
                    DateTime datetime = (DateTime)timeEditBackupFromWeek.EditValue;
                    TimeOnly time = TimeOnly.FromDateTime(datetime);
                    m_backup_module_system.m_backup_from_selected.m_week_update_time = time;
                }
            }
            m_threading = false;
        }

        private void bbtnBackupFromScheduleAdd_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            if (timeEditBackupFromSchedule.EditValue is DateTime && dateEditBackupFromSchedule.EditValue is DateTime)
            {
                if (timeEditBackupFromSchedule.EditValue != null && dateEditBackupFromSchedule.EditValue != null)
                {
                    DateTime time = (DateTime)timeEditBackupFromSchedule.EditValue;
                    DateTime date = (DateTime)dateEditBackupFromSchedule.EditValue;
                    DateTime combinedDateTime = new DateTime(date.Year, date.Month, date.Day,
                                                 time.Hour, time.Minute, time.Second);
                    m_backup_module_system.m_backup_from_selected.h_add_schedule(combinedDateTime, out string message);
                    BackupFromSelectedUpdate();

                    if (Func.is_success(message))
                    {
                        MessageBox.Show($"{combinedDateTime.ToString()} 의 예약이 추가 되었습니다.");
                    }
                    else
                    {
                        MessageBox.Show(message);
                    }

                }
            }
            m_threading = false;
        }

        private void bbtnBackupFromCopyNow_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            m_backup_module_system.m_backup_from_selected.h_backup(out string message);
            BackupToListUpdate();
            if (Func.is_success(message))
            {
                MessageBox.Show($"{DateTime.Now.ToString()} 의 현재 시간으로 백업 되었습니다.");
            }
            else
            {
                MessageBox.Show(message);
            }
            m_threading = false;
        }

        private void ckbSun_EditValueChanged(object sender, EventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            m_backup_module_system.m_backup_from_selected.m_sun = (bool)ckbSun.EditValue;
            m_threading = false;
        }
        private void ckbMon_EditValueChanged(object sender, EventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            m_backup_module_system.m_backup_from_selected.m_mon = (bool)ckbMon.EditValue;
            m_threading = false;
        }
        private void ckbTue_EditValueChanged(object sender, EventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            m_backup_module_system.m_backup_from_selected.m_tue = (bool)ckbTue.EditValue;
            m_threading = false;
        }

        private void ckbWed_EditValueChanged(object sender, EventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            m_backup_module_system.m_backup_from_selected.m_wed = (bool)ckbWed.EditValue;
            m_threading = false;
        }

        private void ckbThu_EditValueChanged(object sender, EventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            m_backup_module_system.m_backup_from_selected.m_thu = (bool)ckbThu.EditValue;
            m_threading = false;
        }

        private void ckbFri_EditValueChanged(object sender, EventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            m_backup_module_system.m_backup_from_selected.m_fri = (bool)ckbFri.EditValue;
            m_threading = false;
        }

        private void ckbSat_EditValueChanged(object sender, EventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            m_backup_module_system.m_backup_from_selected.m_sat = (bool)ckbSat.EditValue;
            m_threading = false;
        }

        private void ckbRun_EditValueChanged(object sender, EventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            m_backup_module_system.m_backup_from_selected.m_run = (bool)ckbRun.EditValue;
            m_threading = false;
        }

        private void lstbBackupFrom_SelectedValueChanged(object sender, EventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            if (lstbBackupFrom.SelectedItem is BackupFrom selectedBackup)
            {
                m_backup_module_system.m_backup_from_selected = selectedBackup;
                m_backup_module_system.m_backup_to_selected = null;
            }
            BackupToListUpdate();
            BackupFromSelectedUpdate();
            BackupToSelectedUpdate();
            m_threading = false;
        }

        private void lstbBackupTo_SelectedValueChanged(object sender, EventArgs e)
        {

            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            if (lstbBackupTo.SelectedItem is BackupTo selectedBackup)
            {
                m_backup_module_system.m_backup_to_selected = selectedBackup;
            }
            BackupToSelectedUpdate();
            m_threading = false;
        }

        private void bbtnBackupToRestore_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            if (m_backup_module_system.m_backup_from_selected != null &&
                m_backup_module_system.m_backup_to_selected != null)
            {
                m_backup_module_system.m_backup_from_selected.h_restore
                    (m_backup_module_system.m_backup_to_selected,out string message);

                if (Func.is_success(message))
                {
                    MessageBox.Show($"{m_backup_module_system.m_backup_to_selected.ToString()} 백업 파일으로 복원 되었습니다.");
                }
                else
                {
                    MessageBox.Show(message);
                }
            }
            else
            {
                MessageBox.Show("복원시킬 백업 파일을 선택 해 주세요.");
            }
            m_threading = false;
        }

        private void bbtnBackupToDelete_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            if (m_backup_module_system.m_backup_to_selected != null &&
                m_backup_module_system.m_backup_from_selected != null)
            {
                string name = m_backup_module_system.m_backup_from_selected.ToString();
                m_backup_module_system.m_backup_from_selected.h_delete_backup_to(
                    m_backup_module_system.m_backup_to_selected, out string message
                    );
                m_backup_module_system.m_backup_to_selected = null;
                BackupToSelectedUpdate(); 
                if (Func.is_success(message))
                {
                    MessageBox.Show($"{name} 의 백업 파일이 삭제 되었습니다.");
                }
                else
                {
                    MessageBox.Show(message);
                }
            }
            m_threading = false;
        }


        private void bbtnBackupSettingSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            if (m_db_name == null)
            {
                MessageBox.Show("저장 시킬 이름을 입력해 주세요");
                m_threading = false;
                return;
            }
            if (m_db_name == "")
            {
                MessageBox.Show("저장 시킬 이름을 입력해 주세요");
                m_threading = false;
                return;
            }
            m_backup_module_system.h_db_save(m_db_name, out string message);
            MessageBox.Show(message);
            DataBaseUpdate();
            m_threading = false;
        }

        private void bbtnBackupSettingLoad_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            if (m_db_selected == null)
            {
                MessageBox.Show("선택된 Setting 값이 없습니다 !");
                m_threading = false;
                return;
            }
            else
            {
                m_backup_module_system = BackupModuleSystem.BackupModuleSystem.h_db_load(m_db_selected.id, out string message);
                MessageBox.Show(message);
                BackupToSelectedUpdate();
                BackupToListUpdate();
                BackupFromListUpdate();
                BackupFromSelectedUpdate();
                DataBaseUpdate();
            }
            m_threading = false;
        }

        private void bbtnBackupSettingUpdate_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            DataBaseUpdate();
            m_threading = false;
        }

        private void listBackupSettingSettingList_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            int selectedIndex = listBackupSettingSettingList.Strings.IndexOf(e.Item.Caption); // 선택된 항목의 인덱스
            if (selectedIndex < 0 || selectedIndex >= m_db_all_load.Count)
            {
                m_threading = false;
                return;
            }
            m_db_selected = m_db_all_load[selectedIndex];
            lblBackupSettingSelectedSetting.Caption = m_db_selected.name;
            m_threading = false;
        }

        private void beiBackupSettingName_EditValueChanged(object sender, EventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            string name = beiBackupSettingName.EditValue?.ToString();
            if (name != null && name != "")
            {
                lblBackupSettingName.Caption = name;
                m_db_name = name;
            }
            m_threading = false;
        }

        private void listBackupSettingSettingList_ListItemClick(object sender, ListItemClickEventArgs e)
        {
            if (m_threading)
            {
                threading_alarm();
                return;
            }
            m_threading = true;
            int selectedIndex = e.Index; // 선택된 항목의 인덱스 가져오기

            // 인덱스가 유효한지 확인
            if (selectedIndex < 0 || selectedIndex >= m_db_all_load.Count)
            {
                m_threading = false;
                return;
            }
            m_db_selected = m_db_all_load[selectedIndex];
            lblBackupSettingSelectedSetting.Caption = m_db_selected.name;

            Console.WriteLine($"[SUCCESS] 선택된 데이터: {m_db_selected.name}");
            m_threading = false;
        }
    }
}
