using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebAppVen.Models;
using WebAppVen.FileService;

namespace WebAppVen.Repository.UR
{
    public class UR_UploadRepository : JCLib.Mvc.BaseRepository
    {
        public UR_UploadRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public int Create(IEnumerable<UR_UPLOAD> ur_upload)
        {
            string sql = @"INSERT INTO UR_UPLOAD (FG, UK, TUSER, FP, FN, FT, FS, ST, IP, UK1, UK2, UK3, UK4, UK5) VALUES 
                        (@FG, @UK, @TUSER, @FP, @FN, @FT, @FS, @ST, @IP, @UK1, @UK2, @UK3, @UK4, @UK5)";
            return DBWork.Connection.Execute(sql, ur_upload, DBWork.Transaction);
        }

        public IEnumerable<UR_UPLOAD> GetFilesByUploadKey(string upload_key)
        {
            string where = "";
            DynamicParameters dp = new DynamicParameters();
            string[] UKv = upload_key.Split(',');
            int ub = UKv.Length;
            ub = ub > 5 ? 5 : ub;
            for (int i = 0; i < ub; i++)
                if (UKv[i] != "")
                {
                    int j = i + 1;
                    dp.Add(string.Format("UK{0}", j), UKv[i]);
                    where += string.Format(" AND UK{0} = @UK{0} ", j);
                }

            if (where == "") where = " AND 1=2 ";

            string sql = @"SELECT FG, UK, TUSER, FP, FN, FT, FS, FC, ST, IP, FD, 
                           (SELECT UNA FROM UR_ID WHERE TUSER = A.TUSER) UNA 
                           FROM UR_UPLOAD A WHERE 1=1 " + where;
            return DBWork.Connection.Query<UR_UPLOAD>(sql, dp, DBWork.Transaction);
        }

        public UR_UPLOAD GetFileByGuid(string file_guid)
        {
            string sql = @"SELECT FG, UK, TUSER, FP, FN, FT, FS, FC, ST, IP, FD, 
                           (SELECT UNA FROM UR_ID WHERE TUSER = A.TUSER) UNA 
                           FROM UR_UPLOAD A WHERE FG = @FG";
            return DBWork.Connection.QueryFirst<UR_UPLOAD>(sql, new { FG = file_guid }, DBWork.Transaction);
        }

        public int DeleteByFG(string file_guid)
        {
            IFileService fs_client = new FileServiceClient();
            int total_physical_deleted = fs_client.DeleteFile(file_guid);

            string sql = @"DELETE FROM UR_UPLOAD WHERE FG = @FG";
            return DBWork.Connection.Execute(sql, new { FG = file_guid }, DBWork.Transaction);
        }

        public int DeleteByUK(string upload_key)
        {
            string sql1 = @"SELECT FG FROM UR_UPLOAD WHERE UK = @UK";
            IEnumerable<UR_UPLOAD> uploadFiles = DBWork.Connection.Query<UR_UPLOAD>(sql1, new { UK = upload_key }, DBWork.Transaction);

            int total_physical_deleted = 0;
            foreach (UR_UPLOAD uu in uploadFiles)
            {
                IFileService fs_client = new FileServiceClient();
                total_physical_deleted += fs_client.DeleteFile(uu.FG.ToString());
            }

            string sql = @"DELETE FROM UR_UPLOAD WHERE UK = @UK";
            return DBWork.Connection.Execute(sql, new { UK = upload_key }, DBWork.Transaction);
        }

        public int UpdateFD(string file_guid, string file_desc)
        {
            string sql = @"UPDATE UR_UPLOAD SET FD = @FD WHERE FG = @FG";
            return DBWork.Connection.Execute(sql, new { FD = file_desc, FG = file_guid }, DBWork.Transaction);
        }

        public int UpdateVLD(string file_guid, bool file_valid)
        {
            string sql = @"UPDATE UR_UPLOAD SET VLD = @VLD WHERE FG = @FG";
            return DBWork.Connection.Execute(sql, new { VLD = file_valid, FG = file_guid }, DBWork.Transaction);
        }

        public string CopyFileSAP(FileDirSAP file_dir_sap, string file_guid, string doc_id = "")
        {
            string sql_ext = @"SELECT FN FROM UR_UPLOAD WHERE FG=@FG";
            object fn = DBWork.Connection.ExecuteScalar(sql_ext, new { FG = file_guid }, DBWork.Transaction);

            if (fn != null)
            {
                string file_name = (doc_id == "") ? fn.ToString() : string.Format("{0}_{1}", doc_id, fn);
                string sub_dir = "";
                switch (file_dir_sap)
                {
                    case FileDirSAP.inqu: sub_dir = "inqu"; break;
                    case FileDirSAP.quot: sub_dir = "quot"; break;
                }
                IFileService fs_client = new FileServiceClient();
                int total_physical_copied = 0;
                total_physical_copied = fs_client.CopyFile(sub_dir, file_guid, file_name);

                string root_path = @"\\10.1.1.110\epms";
                string filePathDes = System.IO.Path.Combine(root_path, sub_dir, file_name);

                if (total_physical_copied == 1)
                {
                    string sql = @"UPDATE UR_UPLOAD SET SAPFN = @SAPFN WHERE FG = @FG";
                    int db_updated = DBWork.Connection.Execute(sql, new { SAPFN = filePathDes, FG = file_guid }, DBWork.Transaction);
                    if (db_updated == 1)
                        return filePathDes;
                }
            }
            return "";
        }

        public int CopyFilesByUK(string old_upload_key, string new_upload_key)
        {
            string sql_ins = @"INSERT INTO UR_UPLOAD (FG, TUSER, FP, FN, FT, FS, FC, ST, IP, UK, UK1, UK2, UK3, UK4, UK5, FD, SAPFN)
                SELECT @NEW_FG, TUSER, FP, FN, FT, FS, FC, ST, IP, @NEW_UK, @UK1, @UK2, @UK3, @UK4, @UK5, FD, SAPFN FROM UR_UPLOAD 
                WHERE UK=@UK AND FG=@FG";

            string sql = @"SELECT FG FROM UR_UPLOAD WHERE UK = @UK";
            IEnumerable<UR_UPLOAD> uploadFiles = DBWork.Connection.Query<UR_UPLOAD>(sql, new { UK = old_upload_key }, DBWork.Transaction);

            string[] UKv = new_upload_key.Split(',');
            int ub = UKv.Length;
            var UK1 = ub > 0 ? UKv[0] : "";
            var UK2 = ub > 1 ? UKv[1] : "";
            var UK3 = ub > 2 ? UKv[2] : "";
            var UK4 = ub > 3 ? UKv[3] : "";
            var UK5 = ub > 4 ? UKv[4] : "";

            int total_physical_copied = 0;
            foreach (UR_UPLOAD uu in uploadFiles)
            {
                var new_fg = Guid.NewGuid().ToString();
                DBWork.Connection.Execute(sql_ins,
                    new
                    {
                        NEW_FG = new_fg,
                        NEW_UK = new_upload_key,
                        UK1 = UK1,
                        UK2 = UK2,
                        UK3 = UK3,
                        UK4 = UK4,
                        UK5 = UK5,
                        UK = old_upload_key,
                        FG = uu.FG
                    }, DBWork.Transaction);

                IFileService fs_client = new FileServiceClient();
                total_physical_copied += fs_client.CopyFileByFG(uu.FG.ToString(), new_fg);
            }

            return total_physical_copied;
        }
    }

    public enum FileDirSAP
    {
        /// <summary>
        /// 詢價目錄 - inqu
        /// </summary>
        inqu,
        /// <summary>
        /// 報價目錄 - quot
        /// </summary>
        quot
    }
}