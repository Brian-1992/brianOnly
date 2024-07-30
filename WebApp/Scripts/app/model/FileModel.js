//定義FileModel資料模型
Ext.define('WEBAPP.model.FileModel', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'SL', type: 'boolean' },//是否選取
        { name: 'DN', type: 'boolean' },//是否已上傳
        { name: 'UK', type: 'string' },//Upload Key
        { name: 'FG', type: 'string' },//檔案GUID
        { name: 'FN', type: 'string' },//檔案名稱
        { name: 'FD', type: 'string' },//檔案說明
        { name: 'FT', type: 'string' },//副檔名
        { name: 'FP', type: 'string' },//檔案位址
        { name: 'ST', type: 'string' },//檔案狀態 N: Not Confirmed, Y: Confirmed
        { name: 'FC', type: 'date' },//建立日期
        { name: 'UNA', type: 'string' }//建立人員
    ]
});