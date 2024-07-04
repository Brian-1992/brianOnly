﻿Ext.define('WEBAPP.store.AB0089_1', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.AB0089_1',
    pageSize: 20, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'ProcDateTime', direction: 'ASC' }], // 預設排序欄位
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AB0089/All_1',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});