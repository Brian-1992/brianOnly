﻿Ext.define('WEBAPP.store.AB.AB0024VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.ME_DOCM',
        'WEBAPP.model.ME_DOCD',
    ],
    stores: {
        ME_DOCM: {
            model: 'WEBAPP.model.ME_DOCM',
            pageSize: 50, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'DOCNO', direction: 'DESC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AB0024/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        ME_DOCD: {
            model: 'WEBAPP.model.ME_DOCD',
            pageSize: 50,
            remoteSort: true,
            sorters: [{ property: 'SEQ', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/AB0024/AllMeDocd',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                    //totalProperty: 'rc'
                }
            }
        }
    }

});