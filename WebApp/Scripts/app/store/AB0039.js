﻿Ext.define('WEBAPP.store.AB0039', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AB0039',
    pageSize: 9999, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'ORDERCODE', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0039/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});