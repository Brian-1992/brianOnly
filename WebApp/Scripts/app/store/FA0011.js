﻿Ext.define('WEBAPP.store.FA0011', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.ItemAllocation',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/FA0011/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});