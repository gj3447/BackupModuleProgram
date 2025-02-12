 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupModuleSystem
{
    public static class Func
    {
        public static string success_string = "TASK COMPLETE";
        public static string error_string = "ERROR:";

        public static bool is_success(string str)
        {
            return str != null && str.Contains(success_string);
        }
        public static string h_copy(string read_path, string write_path)
        {
            if(File.Exists(read_path))
            {
                if (Directory.Exists(write_path))
                    return error_string + "복사할 위치에 디렉터리가 존재합니다";
                return h_file_copy(read_path, write_path);
            }
            if(Directory.Exists(read_path))
            {
                if (File.Exists(write_path))
                    return error_string + "복사할 위치에 파일이 존재합니다";
                return h_directory_copy(read_path, write_path);
            }
            return error_string + "복사하려는 위치에 아무것도 존재하지 않습니다";
        }
        public static string h_copy_hard(string read_path , string write_path)
        {
            if (File.Exists(read_path))
            {
                if (Directory.Exists(write_path))
                    return error_string + "복사할 위치에 디렉터리가 존재합니다";
                return h_file_copy(read_path, write_path);
            }
            if (Directory.Exists(read_path))
            {
                if (File.Exists(write_path))
                    return error_string + "복사할 위치에 파일이 존재합니다";
                return h_directory_copy_hard(read_path, write_path);
            }
            return error_string + "복사하려는 위치에 아무것도 존재하지 않습니다";
        }
        public static string h_file_copy(string read_path, string write_path)
        {
            try
            {
                File.Copy(read_path, write_path, true); // 덮어쓰기 허용
                return success_string;
            }
            catch (Exception ex)
            {
                return error_string + " " + ex.Message;
            }
        }
        public static string h_directory_copy_hard(string read_path,string write_path)
        {
            try
            {
                if (Directory.Exists(write_path))
                {
                    Directory.Delete(write_path, true);
                }
                return h_directory_copy(read_path, write_path);
            }
            catch (Exception ex)
            {
                return error_string + ex.Message;
            }
        }
        public static string h_directory_copy(string read_path,string write_path)
        {
            try
            {
                if (!Directory.Exists(write_path))
                {
                    Directory.CreateDirectory(write_path);
                }
                foreach (string file in Directory.GetFiles(read_path))
                {
                    string destFile = Path.Combine(write_path, Path.GetFileName(file));
                    File.Copy(file, destFile, true); // 덮어쓰기 허용
                }
                foreach (string dir in Directory.GetDirectories(read_path))
                {
                    string destDir = Path.Combine(write_path, Path.GetFileName(dir));
                    h_directory_copy_recursion(dir, destDir); // 재귀적으로 디렉터리 복사
                }
                return success_string;
            }
            catch (Exception ex)
            {
                return error_string + ex.Message;
            }
        }
        public static string h_directory_copy_recursion(string read_path, string write_path)
        {
            if (!Directory.Exists(write_path))
            {
                Directory.CreateDirectory(write_path);
            }
            foreach (string file in Directory.GetFiles(read_path))
            {
                string destFile = Path.Combine(write_path, Path.GetFileName(file));
                File.Copy(file, destFile, true); // 덮어쓰기 허용
            }
            foreach (string dir in Directory.GetDirectories(read_path))
            {
                string destDir = Path.Combine(write_path, Path.GetFileName(dir));
                h_directory_copy(dir, destDir); // 재귀적으로 디렉터리 복사
            }
            return success_string;
        }
    }
}
