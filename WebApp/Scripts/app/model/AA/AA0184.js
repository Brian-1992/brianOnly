Ext.define('WEBAPP.model.AA.AA0184', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'DOCNO', type: 'string' },//申請單號
        { name: 'MAT_CLASS', type: 'string' },//物料分類
        { name: 'COMBITEM', type: 'string' },//狀態
        { name: 'FRWH', type: 'string' },//出庫庫房
        { name: 'TOWH', type: 'string' },//入庫庫房
        { name: 'APPTIME', type: 'string' },//申請時間
        { name: 'MMCODE', type: 'string' },//院內碼
        { name: 'MMNAME_C', type: 'string' },//中文名稱
        { name: 'MMNAME_E', type: 'string' },//英文名稱
        { name: 'BASE_UNIT', type: 'string' },//單位
        { name: 'APPQTY', type: 'string' },//申請數量
        { name: 'APVQTY', type: 'string' },//核可數量
        { name: 'DIS_TIME', type: 'string' }//核撥日期
    ]
});