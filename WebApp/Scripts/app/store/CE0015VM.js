Ext.define('WEBAPP.store.CE0015VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.CHK_GRADE2_P'
    ],
    stores: {
        Include: {
            model: 'WEBAPP.model.CHK_GRADE2_P',
            pageSize: 999999, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0015/Include',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        All: {
            model: 'WEBAPP.model.CHK_GRADE2_P',
            pageSize: 999999, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0015/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        PreviousP: {
            model: 'WEBAPP.model.CHK_GRADE2_P',
            pageSize: 999999, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0015/PreviousP',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        PreviousS: {
            model: 'WEBAPP.model.CHK_GRADE2_P',
            pageSize: 999999, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0015/PreviousS',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
    }
});