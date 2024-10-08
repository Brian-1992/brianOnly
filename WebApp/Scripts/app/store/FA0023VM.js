﻿Ext.define('WEBAPP.store.FA0023VM', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.FA0023M',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'WH_NO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        timeout: 1200000,
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/FA0023/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});