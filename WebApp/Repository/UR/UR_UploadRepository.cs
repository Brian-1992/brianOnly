using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using WebApp.FileService;

namespace WebApp.Repository.UR
{
    public class UR_UploadRepository : JCLib.Mvc.BaseRepository
    {
        public UR_UploadRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public int Create(IEnumerable<UR_UPLOAD> ur_upload)
        {
            /*
            string sql = @"INSERT INTO UR_UPLOAD (FG, UK, TUSER, FP, FN, FT, FS, ST, IP, UK1, UK2, UK3, UK4, UK5) VALUES 
                        (:FG, :UK, :TUSER, :FP, :FN, :FT, :FS, :ST, :IP, :UK1, :UK2, :UK3, :UK4, :UK5)";
                        */
            string sql = @"INSERT INTO UR_UPLOAD (FG, UK, TUSER, FP, FN, FT, FS, FC, ST, IP) VALUES 
                        (:FG, :UK, :TUSER, :FP, :FN, :FT, :FS, SYSDATE, :ST, :IP)";
            return DBWork.Connection.Execute(sql, ur_upload, DBWork.Transaction);
        }

        public IEnumerable<UR_UPLOAD> GetFilesByUploadKey(string upload_key)
        {
            string where = "";
            DynamicParameters dp = new DynamicParameters();

            /*
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
            */

            where = " AND UK=:UK";
            dp.Add("UK", upload_key);

            string sql = @"SELECT FG, UK, TUSER, FP, FN, FT, FS, FC, ST, IP, FD, 
                           (SELECT UNA FROM UR_ID WHERE TUSER = A.TUSER) UNA 
                           FROM UR_UPLOAD A WHERE 1=1 " + where;
            return DBWork.Connection.Query<UR_UPLOAD>(sql, dp, DBWork.Transaction);
        }

        public UR_UPLOAD GetFileByGuid(string file_guid)
        {
            string sql = @"SELECT FG, UK, TUSER, FP, FN, FT, FS, FC, ST, IP, FD, 
                           (SELECT UNA FROM UR_ID WHERE TUSER = A.TUSER) UNA 
                           FROM UR_UPLOAD A WHERE FG =:FG";
            return DBWork.Connection.QueryFirst<UR_UPLOAD>(sql, new { FG = file_guid }, DBWork.Transaction);
        }

        /// <summary>
        /// 把uploadKey下的檔案狀態變更為Y (已確認儲存)
        /// </summary>
        /// <param name="uploadKey"></param>
        /// <returns></returns>
        public int Confirm(string uploadKey)
        {
            string sql = @"UPDATE UR_UPLOAD SET ST='Y' WHERE UK=:UK AND ST='N'";

            return DBWork.Connection.Execute(sql, new { UK = uploadKey }, DBWork.Transaction);
        }

        public int DeleteByFG(string file_guid)
        {
            IFileService fs_client = new FileServiceClient();
            int total_physical_deleted = fs_client.DeleteFile(file_guid);

            string sql = @"DELETE FROM UR_UPLOAD WHERE FG =:FG";
            return DBWork.Connection.Execute(sql, new { FG = file_guid }, DBWork.Transaction);
        }

        public int DeleteByUK(string upload_key)
        {
            string sql1 = @"SELECT FN FROM UR_UPLOAD WHERE UK = :UK";
            IEnumerable<UR_UPLOAD> uploadFiles = DBWork.Connection.Query<UR_UPLOAD>(sql1, new { UK = upload_key }, DBWork.Transaction);

            int total_physical_deleted = 0;
            foreach(UR_UPLOAD uu in uploadFiles)
            {
                IFileService fs_client = new FileServiceClient();
                total_physical_deleted += fs_client.DeleteFile(uu.FN);
            }

            string sql = @"DELETE FROM UR_UPLOAD WHERE UK = :UK";
            return DBWork.Connection.Execute(sql, new { UK = upload_key }, DBWork.Transaction);
        }

        public int UpdateFD(string file_guid, string file_desc)
        {
            string sql = @"UPDATE UR_UPLOAD SET FD = @FD WHERE FG = @FG";
            return DBWork.Connection.Execute(sql, new { FD = file_desc, FG = file_guid }, DBWork.Transaction);
        }

        public string GetFirstFileGuidByUK(string uploadKey)
        {
            string sql = @"SELECT FG FROM UR_UPLOAD WHERE UK = :UK AND ROWNUM=1";
            object result = DBWork.Connection.ExecuteScalar(sql, new { UK = uploadKey }, DBWork.Transaction);
            return (result ?? "").ToString();
        }

        public int GetFileCountByUK(string uploadKey)
        {
            string sql = @"SELECT COUNT(1) FROM UR_UPLOAD WHERE UK = :UK";
            object result = DBWork.Connection.ExecuteScalar(sql, new { UK = uploadKey }, DBWork.Transaction);
            return int.Parse(result.ToString());
        }
    }
}