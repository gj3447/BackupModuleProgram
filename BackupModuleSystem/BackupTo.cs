using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BackupModuleSystem
{
    public class BackupTo
    {
        [JsonIgnore]
        public BackupFrom m_backup_from { get; set; }
        public string m_write_path { get; set; }
        public BACKUP_TO_TYPE m_type { get; set; }

        public BackupTo()
        { }
        public override string ToString()
        {
            return m_write_path;
        }
        public string m_full_write_path
        { get {
                return Path.Combine(m_backup_from.m_write_path, m_write_path);
            } }
        public BackupTo(BackupFrom backup_from,string write_path , BACKUP_TO_TYPE type)
        {
            m_backup_from = backup_from;
            m_write_path = write_path;
            m_type = type;
        }
        public string h_restore()
        {
            return Func.h_copy_hard(m_full_write_path, m_backup_from.m_read_path); 
        }
        public string h_backup()
        {
            return Func.h_copy(m_backup_from.m_read_path, m_full_write_path);
        }
        public string h_delete()
        {
            if(Directory.Exists(m_full_write_path))
            {
                Directory.Delete(m_full_write_path, true);
                return Func.success_string;
            }
            if(File.Exists(m_full_write_path))
            {
                File.Delete(m_full_write_path);
                return Func.success_string;
            }
            return Func.error_string + "null write";
        }
        public enum BACKUP_TO_TYPE
        {
            BACKUP,
            SCHEDULE,
            WEEK,
        }
    }
}