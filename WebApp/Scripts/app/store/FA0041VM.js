Ext.define('WEBAPP.store.FA0041VM', {
    extend: 'Ext.app.ViewModel',
    //alias: 'viewmodel.AA0015',
    requires: [
        'WEBAPP.model.FA0041'
    ],
    stores: {
        All: {
            model: 'WEBAPP.model.FA0041',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'INID', direction: 'ASC' }, { property: 'WH_NO', direction: 'ASC' }, { property: 'CHK_TYPE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/FA0041/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        DoneDatas: {
            model: 'WEBAPP.model.FA0041',
            //autoLoad: true,
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'INID', direction: 'ASC' }, { property: 'WH_NO', direction: 'ASC' }, { property: 'CHK_TYPE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/FA0041/DoneDatas',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        UndoneDatas: {
            model: 'WEBAPP.model.FA0041',
            //autoLoad: true,
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'INID', direction: 'ASC' }, { property: 'WH_NO', direction: 'ASC' }, { property: 'CHK_TYPE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/FA0041/UndoneDatas',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
    }
});