Ext.define('WEBAPP.store.GA0002VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.TC_PURCH_M',
        'WEBAPP.model.TC_PURCH_DL',
        'WEBAPP.model.TC_INVQMTR'
    ],
    stores: {
        Master: {
            model: 'WEBAPP.model.TC_PURCH_M',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'PUR_NO', direction: 'DESC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/GA0002/Master',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        Details: {
            model: 'WEBAPP.model.TC_PURCH_DL',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/GA0002/Detail',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        Invqmtrs: {
            model: 'WEBAPP.model.TC_INVQMTR',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }, { property: 'PUR_SEQ', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/GA0002/Invqmtrs',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        Orders: {
            model: 'WEBAPP.model.TC_PURCH_M',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'PUR_NO', direction: 'DESC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/GA0002/Orders',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }
    }
});