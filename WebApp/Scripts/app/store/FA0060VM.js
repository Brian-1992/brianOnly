Ext.define('WEBAPP.store.FA0060VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.FA0060Data'
    ],
    stores: {
        GetData: {
            model: 'WEBAPP.model.FA0060Data',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'GRP_NO', direction: 'ASC' }, { property: 'INID', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/FA0060/Data',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        Details: {
            model: 'WEBAPP.model.FA0060Data',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'DATA_YM', direction: 'ASC' }, { property: 'GRP_NO', direction: 'ASC' }, { property: 'INID', direction: 'ASC' },{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                timeout: 900000,
                url: '/api/FA0060/Details',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        GetT3Data: {
            model: 'WEBAPP.model.FA0060Data',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'GRP_NO', direction: 'ASC' }, { property: 'INID', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/FA0060/GetT3Data',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
    }

});

