Ext.define('WEBAPP.model.ME_AB0046', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'VISITKIND', type: 'string' },           // 門住診別 (0.不分,1.住院,2.門診,3.急診)     
        { name: 'LOCATION', type: 'string' },            // 動向碼 (診間/病房) 
        { name: 'FREQNO', type: 'string' },              // 院內頻率
        { name: 'BEGINTIME', type: 'string' },           // 起始時間 (例行時間起)       
        { name: 'ENDTIME', type: 'string' },             // 迄止時間 (例行時間迄)   
        { name: 'DEFAULTSTOCKCODE', type: 'string' },    // 預設扣庫別(優先扣庫) 
        { name: 'ROUTINESTOCKCODE', type: 'string' },    // 例行庫扣庫別  
        { name: 'EXCEPTSTOCKCODE', type: 'string' },     // 非例行庫扣庫別  
        { name: 'TAKEOUTSTOCKCODE', type: 'string' },    // 出院帶藥扣庫別 
        { name: 'TPNSTOCKCODE', type: 'string' },        // TPN扣庫別  
        { name: 'PCASTOCKCODE', type: 'string' },        // PCA扣庫別  
        { name: 'CHEMOSTOCKCODE', type: 'string' },      // CHEMO扣庫別 (化療扣庫)
        { name: 'RESEARCHSTOCKCODE', type: 'string' },   // 研究用藥扣庫別
        { name: 'RETURNSTOCKCODE', type: 'string' },     // 退藥庫別 
        { name: 'CREATEDATETIME', type: 'string' },      // 記錄建立日期/時間 
        { name: 'CREATEOPID', type: 'string' },          // 記錄建立人員 
        { name: 'PROCDATETIME', type: 'string' },        // 記錄處理日期/時間
        { name: 'PROCOPID', type: 'string' }             // 記錄處理人員
    ]
});