Ext.define('WEBAPP.model.ME_AB0079', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'ORDERCODE', type: 'string' },    // 院內碼(畫面)
        { name: 'ORDERENGNAME', type: 'string' }, // 英文名稱
        { name: 'CREATEYM', type: 'string' },     // 查詢月份
        { name: 'ORDERDR', type: 'string' },      // 醫師代碼  
        { name: 'CHINNAME', type: 'string' },     // 醫師姓名 
        { name: 'SECTIONNO', type: 'string' },    // 科室 
        { name: 'SECTIONNAME', type: 'string' },  // 科室名 
        { name: 'SUMQTY', type: 'string' },       // 醫囑(住)消耗量  
        { name: 'SUMAMOUNT', type: 'string' },    // 醫囑(住)消耗金額
        { name: 'OPDQTY', type: 'string' },       // 醫囑(門)消耗量 
        { name: 'OPDAMOUNT', type: 'string' },    // 醫囑(門)消耗金額  
        { name: 'DSM', type: 'string' },          // D:醫師 S:科室 M:藥品
    ]
});