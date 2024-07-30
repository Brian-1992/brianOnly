Ext.define('WEBAPP.store.BD0011VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.BD0011',
        'WEBAPP.model.ERROR_LOG',
        'WEBAPP.model.BD0011'
    ],
    stores: {
        Master: {
            model: 'WEBAPP.model.BD0011',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'SEND_DT', direction: 'ASC' }, { property: 'DOCID', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/BD0011/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        Agens: {
            model: 'WEBAPP.model.BD0011',
            pageSize: 999999, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'AGEN_NO', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/BD0011/GetAGEN',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        ErrorLog: {
            model: 'WEBAPP.model.ERROR_LOG',
            pageSize: 999999, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'LOGTIME', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/BD0011/ErrorLog',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
    }

    //extend: 'Ext.data.Store',
    //model: 'WEBAPP.model.BD0011',
    //pageSize: 999999, // 每頁顯示筆數
    //remoteSort: true,
    //sorters: [{ property: 'AGEN_NO', direction: 'ASC' }], // 預設排序欄位
    //proxy: {
    //    type: 'ajax',
    //    actionMethods: {
    //        read: 'POST' // by default GET
    //    },
    //    url: '/api/BD0011/GetAGEN',
    //    reader: {
    //        type: 'json',
    //        rootProperty: 'etts',
    //        totalProperty: 'rc'
    //    }
    //}
});