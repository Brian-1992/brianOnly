Ext.define('WEBAPP.model.UR_ID', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'TUSER', type: 'string' },        // 人員代碼
        { name: 'INID', type: 'string' },         // 責任中心代碼
        { name: 'PA', type: 'string' },           
        { name: 'SL', type: 'string' },           // SALT
        { name: 'UNA', type: 'string' },          // 中文姓名
        { name: 'UENA', type: 'string' },         // 英文姓名
        { name: 'IDDESC', type: 'string' },       // 備註
        { name: 'EMAIL', type: 'string' },        // EMAIL
        { name: 'EXT', type: 'string' },          // 分機
        { name: 'BOSS', type: 'string' },         // 是否主管
        { name: 'TITLE', type: 'string' },        // 職稱
        { name: 'FAX', type: 'string' },          // 傳真
        { name: 'FL', type: 'string' },           // 是否有效(1有效0無效)
        { name: 'TEL', type: 'string' },          // 電話
        { name: 'CREATE_TIME', type: 'string' },  // 建立時間
        { name: 'CREATE_USER', type: 'string' },  // 建立人員代碼
        { name: 'UPDATE_TIME', type: 'string' },  // 異動時間
        { name: 'UPDATE_USER', type: 'string' },  // 異動人員代碼
        { name: 'UPDATE_IP', type: 'string' }     // 異動IP
    ]
});