Ext.define('WEBAPP.store.CE0024VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.CHK_GRADE2_P',
        'WEBAPP.model.CE0027'
    ],
    stores: {
        All: {
            model: 'WEBAPP.model.CHK_GRADE2_P',
            pageSize: 999999, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                timeout: 120000,
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0024/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        ChkResult: {
            model: 'WEBAPP.model.CE0027',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                timeout: 120000,
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0024/GetChkResult',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }
    }
});