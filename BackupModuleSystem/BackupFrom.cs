using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupModuleSystem
{
    public class BackupFrom
    {
        #region field
        public string m_read_path { get; set; }
        public string m_write_path { get; set; }
        public List<BackupTo> m_backup_to_list { get; set; } = new List<BackupTo>();
        public List<DateTime> m_schedule_update_list { get; set; } = new List<DateTime>();

        public bool m_run { get; set; } = false;
        public bool m_sun { get; set; } = false;
        public bool m_mon { get; set; } = false;
        public bool m_tue { get; set; } = false;
        public bool m_wed { get; set; } = false;
        public bool m_thu { get; set; } = false;
        public bool m_fri { get; set; } = false;
        public bool m_sat { get; set; } = false;
        public DateTime m_last_week_update_time { get; set; } = DateTime.MinValue;
        public TimeOnly m_week_update_time { get; set; } = TimeOnly.MinValue;
        #endregion

        public override string ToString()
        {
            return m_read_path + "=>" + m_write_path;
        }
        public bool h_add_schedule(DateTime schedule_time , out string message)
        {
            if(schedule_time< DateTime.Now)
            {
                message = Func.error_string + "과거 시간으로 예약 할 수 없습니다.";
                return false;
            }
            m_schedule_update_list.Add(schedule_time);
            message = Func.success_string;
            return true;
        }
        public BackupTo h_backup(out string message)
        {
            BackupTo result = null;
            message = "";
            result = new BackupTo(this,
                    datetime2string(DateTime.Now, BackupTo.BACKUP_TO_TYPE.BACKUP),
                    BackupTo.BACKUP_TO_TYPE.BACKUP);
            m_backup_to_list.Add(result);
            message = result.h_backup();
            return result;
        }
        public List<BackupTo> h_update(out List<string> message_list)
        {
            List<BackupTo> result = new List<BackupTo>();

            BackupTo schedule_backup = h_schedule_update(out string schedule_message);
            BackupTo week_backup = h_week_update(out string week_message);

            message_list = new List<string>();
            message_list.Add(week_message);
            message_list.Add(schedule_message);
            result.Add(week_backup);
            result.Add(schedule_backup);

            return result;
        }
        public void h_restore(BackupTo restore , out string message)
        {
            if(restore == null)
            {
                message = Func.error_string + "restore 할 BackupTo 가 없습니다";
                return;
            }
            string write_path = Path.Combine(m_write_path, restore.m_write_path);
            message = Func.h_copy_hard(write_path, m_read_path);
        }
        private BackupTo h_week_update(out string message)
        {
            BackupTo result = null;
            message = "";
            if (is_week)
            {
                m_last_week_update_time = DateTime.Now;
                result = new BackupTo(this, 
                    datetime2string(DateTime.Now,BackupTo.BACKUP_TO_TYPE.WEEK),
                    BackupTo.BACKUP_TO_TYPE.WEEK);

                m_backup_to_list.Add(result);
                message = result.h_backup();
            }
            return result;
        }
        private BackupTo h_schedule_update(out string message)
        {
            DateTime datetime = is_schedule;
            BackupTo result = null;
            message = "";
            if (datetime != DateTime.MinValue)
            {
                result = new BackupTo(this,
                    datetime2string(DateTime.Now, BackupTo.BACKUP_TO_TYPE.SCHEDULE),
                    BackupTo.BACKUP_TO_TYPE.SCHEDULE);

                m_backup_to_list.Add(result);
                message = result.h_backup();
            }
            return result;
        }
        private string datetime2string(DateTime datetime,BackupTo.BACKUP_TO_TYPE type)
        {
            string header;
            switch(type)
            {
                case BackupTo.BACKUP_TO_TYPE.BACKUP:
                    header = "BACKUP-";
                    break;
                case BackupTo.BACKUP_TO_TYPE.SCHEDULE:
                    header = "SCHEDULE-";
                    break;
                case BackupTo.BACKUP_TO_TYPE.WEEK:
                    header = "WEEK-";
                    break;
                default: 
                    header = "";
                    break;
            }
            return header + datetime.ToString("yyyy-MM-dd-HH-mm-ss");
        }
        private bool is_week
        {
            get
            {
                if (!m_run)
                    return false;

                bool dow= DateTime.Now.DayOfWeek switch
                {
                    DayOfWeek.Sunday => m_sun,
                    DayOfWeek.Monday => m_mon,
                    DayOfWeek.Tuesday => m_tue,
                    DayOfWeek.Wednesday => m_wed,
                    DayOfWeek.Thursday => m_thu,
                    DayOfWeek.Friday => m_fri,
                    DayOfWeek.Saturday => m_sat,
                    _ => false
                };
                bool time_passed = TimeOnly.FromDateTime(DateTime.Now) > m_week_update_time;
                bool is_not_updated = m_last_week_update_time.Date != DateTime.Now.Date;
                return dow && time_passed && is_not_updated;
            }
        }
        private DateTime is_schedule
        {
            get
            {
                DateTime result = DateTime.MinValue;
                int targetIndex = -1;
                for(int i = 0; i< m_schedule_update_list.Count; i++)
                {
                    if (DateTime.Now >= m_schedule_update_list[i])
                    {
                        result = m_schedule_update_list[i];
                        targetIndex = i;
                    }
                }
                if (targetIndex != -1) // ✅ 유효한 일정이 있으면 제거
                {
                    m_schedule_update_list.RemoveAt(targetIndex);
                }
                return result;
            }
        }
        public void h_delete_backup_to(BackupTo backup_to,out string message)
        {
            m_backup_to_list.Remove(backup_to);
            message = backup_to.h_delete();
        }
        public BackupFrom()
        {
        }
        public BackupFrom(string read_path , string write_path)
        {
            m_read_path = read_path;
            m_write_path = write_path;
        }
        public void h_json_ignore_update()
        {
            foreach(BackupTo e in m_backup_to_list)
            {
                e.m_backup_from = this;
            }
        }
        public enum TYPE
        {
            FILE,
            DIRECTORY,
        }
    }
}
