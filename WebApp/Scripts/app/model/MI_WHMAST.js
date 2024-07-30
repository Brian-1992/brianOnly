Ext.define('WEBAPP.model.MI_WHMAST', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },           // 庫房代碼
        { name: 'WH_NAME', type: 'string' },         // 庫房名稱
        { name: 'WH_KIND', type: 'string' },         // 庫別分類(0藥品庫 1衛材庫 2戰備庫 3疾管局庫)
        { name: 'WH_GRADE', type: 'string' },        // 庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫)
        { name: 'PWH_NO', type: 'string' },          // 上級庫
        { name: 'INID', type: 'string' },            // 責任中心
        { name: 'SUPPLY_INID', type: 'string' },     // 撥補責任中心(庫房)
        { name: 'TEL_NO', type: 'string' },          // 電話分機
        { name: 'CANCEL_ID', type: 'string' },       // 是否作廢
        { name: 'CREATE_DATE', type: 'string' },     // 建立時間
        { name: 'CREATE_USER', type: 'string' },     // 建立人員代碼
        { name: 'UPDATE_DATE', type: 'string' },     // 異動時間
        { name: 'UPDATE_USER', type: 'string' },     // 異動人員代碼
        { name: 'UPDATE_IP', type: 'string' },       // 異動IP
        { name: 'WH_KIND_N', type: 'string' },
        { name: 'WH_GRADE_N', type: 'string' },
        { name: 'PWH_NO_N', type: 'string' },
        { name: 'INID_N', type: 'string' },
        { name: 'SUPPLY_INID_N', type: 'string' },
        { name: 'CANCEL_ID_N', type: 'string' }
    ]
});