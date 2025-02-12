using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using DotNetEnv;
using MySql.Data.MySqlClient;
using System.Runtime.InteropServices.JavaScript;
using System.Xml.Linq;
using Microsoft.Win32;
using Org.BouncyCastle.Utilities;

namespace BackupModuleSystem
{
    public class BackupModuleSystem
    {
        public List<BackupFrom> m_backup_from_list { get; set; }
        
        public BackupFrom m_backup_from_selected { get; set; }
        public BackupTo m_backup_to_selected { get; set; }
        public BackupModuleSystem()
        {
            m_backup_from_list = new List<BackupFrom>();
            m_backup_from_selected = null;
            m_backup_to_selected = null;
        }
        public void h_json_ignore_update()
        {
            foreach (BackupFrom e in m_backup_from_list)
            {
                e.h_json_ignore_update();
            }
        }
        public BackupFrom h_add_backup_from(string from_path, string to_path, out string message)
        {
            if(from_path == null|| from_path=="")
            {
                message = Func.error_string + "from_path 를 입력 해 주세요.";
                return null;
            }
            if(to_path == null|| to_path=="")
            {
                message = Func.error_string + "to_path 를 입력 해 주세요.";
                return null;
            }
            if(File.Exists(from_path))
            {
                message = Func.error_string + "from_path 에 폴더만 선택해 주셔야 합니다.";
                return null;
            }
            if(File.Exists(to_path))
            {
                message = Func.error_string + "to_path 에 폴더만 선택해 주셔야 합니다.";
                return null;
            }
            if(to_path.StartsWith(from_path))
            {
                message = Func.error_string + "to_path 는 from_path 내부에 위치할 수 없습니다";
                return null;
            }
            if(!Directory.Exists(from_path))
            {
                Directory.CreateDirectory(from_path);
            }
            if (!Directory.Exists(to_path))
            {
                Directory.CreateDirectory(to_path);
            }
            BackupFrom result = new BackupFrom(from_path, to_path);
            m_backup_from_list.Add(result);
            message =  Func.success_string;
            m_backup_from_selected = result;
            m_backup_to_selected = null;
            return result;
        }
        public string h_system2json(out string message)
        {
            try
            {
                message = "system2json complete";
                return JsonSerializer.Serialize(this);
            }
            catch (Exception ex)
            {
                message = ("JSON 직렬화 오류: " + ex.Message);
                return string.Empty;
            }
        }

        public static BackupModuleSystem h_json2system(string json, out string message)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // JSON 속성의 대소문자 구분 제거
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
                message = "json2system complete";
                return JsonSerializer.Deserialize<BackupModuleSystem>(json, options);
            }
            catch (Exception ex)
            {
                message = "JSON 역직렬화 오류: " + ex.Message;
                return null;
            }
        }
        public byte[] h_system2blob(out string message)
        {
            try
            {
                byte[] result = Encoding.UTF8.GetBytes(h_system2json(out string temp_message));
                message = $"system2blob complete\r\n{temp_message}";
                return result;
            }
            catch (Exception ex)
            {
                message = "BLOB 변환 오류: " + ex.Message;
                return null;
            }
        }

        public static BackupModuleSystem h_blob2system(byte[] bytes , out string message)
        {
            try
            {
                if (bytes == null || bytes.Length == 0)
                    throw new ArgumentException("입력된 BLOB 데이터가 없습니다.");

                string json = Encoding.UTF8.GetString(bytes);
                BackupModuleSystem result = h_json2system(json, out string temp_message);
                message = $"blob2system complete\r\n{temp_message}\r\n json = {json}\r\n";
                return result;
            }
            catch (Exception ex)
            {
                message = "BLOB에서 객체 변환 오류: " + ex.Message;
                return null;
            }
        }
        public static string get_conn_str()
        {
            Env.Load();
            string dbHost = Env.GetString("DB_HOST");
            string dbPort = Env.GetString("DB_PORT");
            string dbUser = Env.GetString("DB_USER");
            string dbPassword = Env.GetString("DB_PASSWORD");
            string dbName = "TestBackupModule";
            string connStr = $"Server={dbHost};Port={dbPort};Database={dbName};User ID={dbUser};Password={dbPassword};SSL Mode=None;";
            return connStr;
        }
        public static string get_conn_str2
        {
            get
                {
                Env.Load();
                string connStr = $"Server=testdb.c7kisu8i2o79.ap-northeast-2.rds.amazonaws.com;Port=3306;Database=TestBackupModule;User ID=admin;Password=!159FKrudwns;SslMode=None;";
                return connStr;
            }
        }
        public static void h_db_test()
        {
            using (MySqlConnection conn = new MySqlConnection(get_conn_str2))
            {
                try
                {
                    conn.Open();
                    Console.WriteLine("MySQL 연결 성공!");
                }
                catch(Exception ex)
                {
                    Console.WriteLine("오류 발생: " + ex.Message);
                }
            }
        }
        public static string get_uuid
        {
            get
            {
                string key = @"SOFTWARE\Microsoft\Cryptography";
                string valueName = "MachineGuid";

                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key))
                {
                    if (registryKey != null)
                    {
                        object value = registryKey.GetValue(valueName);
                        if (value != null)
                        {
                            return value.ToString();
                        }
                    }
                }
                return "Machine GUID Not Found";
            }
        }
        public static BackupModuleSystem h_db_load(int id, out string message)
        {
            using (MySqlConnection conn = new MySqlConnection(get_conn_str2))
            {
                try
                {
                    conn.Open();
                    message = "MySQL 연결 성공!";
                    string query = "SELECT DATA FROM BackupSettingSystem WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // 데이터가 존재하면
                            {
                                byte[] blob = (byte[])reader["DATA"]; // BLOB 데이터 가져오기
                                string json = Encoding.UTF8.GetString(blob);
                                BackupModuleSystem result = h_blob2system(blob,out string message_temp);
                                message = message_temp;
                                result.h_json_ignore_update();
                                return result;// BLOB을 객체로 변환하여 반환
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    message = "오류 발생: " + ex.Message;
                    return null;
                }
            }
            return null;
        }

        public static List<BackupModuleSystemT> h_db_load_all(out string message)
        {
            string uuid = get_uuid;
            List<BackupModuleSystemT> result = new List<BackupModuleSystemT>();
            using (MySqlConnection conn = new MySqlConnection(get_conn_str2))
            {
                try
                {
                    conn.Open();
                    message = "MySQL 연결 성공!";
                    string query = "SELECT id, TIME, NAME FROM BackupSettingSystem WHERE UUID = @uuid";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uuid", uuid);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BackupModuleSystemT item = new BackupModuleSystemT
                                {
                                    id = reader.GetInt32("id"),
                                    time = reader.GetDateTime("TIME"),
                                    name = reader.GetString("NAME")
                                };
                                result.Add(item);
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    message = "오류 발생: " + ex.Message;
                }
            }
            return result;
        }
        public bool h_db_save(string name, out string message)
        {
            byte[] blob = h_system2blob(out string temp_message);
            message = temp_message;
            string uuid = get_uuid;

            using (MySqlConnection conn = new MySqlConnection(get_conn_str2))
            {
                try
                {
                    conn.Open();
                    // NAME이 존재하는지 확인하는 쿼리
                    string checkQuery = "SELECT COUNT(*) FROM BackupSettingSystem WHERE NAME = @name";

                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@name", name);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            // 데이터 업데이트
                            string updateQuery = @"
                    UPDATE BackupSettingSystem 
                    SET UUID = @uuid, DATA = @blob, TIME = CURRENT_TIMESTAMP
                    WHERE NAME = @name";

                            using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@uuid", uuid);
                                updateCmd.Parameters.AddWithValue("@blob", blob);
                                updateCmd.Parameters.AddWithValue("@name", name);

                                int rowsUpdated = updateCmd.ExecuteNonQuery();
                                message += $"\r\n{rowsUpdated}개의 행이 업데이트되었습니다.";
                                return rowsUpdated > 0;
                            }
                        }
                        else
                        {
                            // 데이터 삽입
                            string insertQuery = @"
                    INSERT INTO BackupSettingSystem (UUID, DATA, TIME, NAME)
                    VALUES (@uuid, @blob, CURRENT_TIMESTAMP, @name)";

                            using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@uuid", uuid);
                                insertCmd.Parameters.AddWithValue("@blob", blob);
                                insertCmd.Parameters.AddWithValue("@name", name);

                                int rowsInserted = insertCmd.ExecuteNonQuery();
                                message += $"\r\n{rowsInserted}개의 행이 추가되었습니다.";
                                return rowsInserted > 0;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    message += "\r\n오류 발생: " + ex.Message;
                    Console.WriteLine("[ERROR] DB 저장 실패: " + ex.Message);
                    return false;
                }
            }
        }
    }
}
