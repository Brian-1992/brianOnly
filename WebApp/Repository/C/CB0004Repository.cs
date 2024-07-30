using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.CB
{
    public class CB0004Repository : JCLib.Mvc.BaseRepository
    {
        public CB0004Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public int Create(BC_BARCODE bc_barcode)
        {
            string sql = @"INSERT INTO bc_barcode (
                            MMCODE,
                            BARCODE,
                            XCATEGORY,
                            CREATE_TIME,
                            CREATE_USER,
                            UPDATE_IP
                        ) VALUES (
                           :MMCODE,
                           :BARCODE,
                           :XCATEGORY,
                           SYSDATE,
                           :CREATE_USER,
                           :UPDATE_IP
                        ) ";

            return DBWork.Connection.Execute(sql, bc_barcode, DBWork.Transaction);
        }

        
        public bool CheckPKDataExists(BC_BARCODE bc_barcode)
        {
            string sql = @"SELECT 1 FROM BC_BARCODE WHERE MMCODE=:MMCODE AND  BARCODE=:BARCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, bc_barcode, DBWork.Transaction) == null);
        }

        public bool CheckMmcodeExists(string MMCODE)
        {
            string sql = @"SELECT 1 FROM MI_MAST WHERE MMCODE=:MMCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE }, DBWork.Transaction) == null);
        }

        public bool CheckBarcodeExists(string MMCODE, string BARCODE)
        {
            string sql = @"SELECT 1 FROM BC_BARCODE WHERE MMCODE=:MMCODE AND BARCODE=:BARCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE, BARCODE = BARCODE }, DBWork.Transaction) == null);
        }

        public bool CheckXcategoryExists(string XCATEGORY)
        {
            string sql = @"SELECT 1 FROM BC_CATEGORY WHERE XCATEGORY=:XCATEGORY ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { XCATEGORY = XCATEGORY }, DBWork.Transaction) == null);
        }
    }
}