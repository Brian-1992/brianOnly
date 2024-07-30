Ext.define('WEBAPP.store.CE0016VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.CHK_MAST',
        'WEBAPP.model.CE0003'
    ],
    stores: {
        Masters: {
            model: 'WEBAPP.model.CE0003',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'CHK_YM', direction: 'DESC' },{ property: 'CHK_PERIOD', direction: 'DESC' }, { property: 'CHK_TYPE_CODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0016/Masters',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },Details: {
            model: 'WEBAPP.model.CE0003',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0016/Details',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }, DetailsMulti: {
            model: 'WEBAPP.model.CHK_MAST',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0016/DetailsMulti',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        //all: {
        //    model: 'WEBAPP.model.CHK_GRADE2_P',
        //    pageSize: 20, // 每頁顯示筆數
        //    remoteSort: true,
        //    sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
        //    proxy: {
        //        type: 'ajax',
        //        actionMethods: {
        //            read: 'POST' // by default GET
        //        },
        //        url: '/api/CE0016/All',
        //        reader: {
        //            type: 'json',
        //            rootProperty: 'etts',
        //            totalProperty: 'rc'
        //        }
        //    }
        //},
        //DetailsMulti: {
        //    model: 'WEBAPP.model.CHK_GRADE2_P',
        //    pageSize: 20, // 每頁顯示筆數
        //    remoteSort: true,
        //    sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
        //    proxy: {
        //        type: 'ajax',
        //        actionMethods: {
        //            read: 'POST' // by default GET
        //        },
        //        url: '/api/CE0003/DetailsMulti',
        //        reader: {
        //            type: 'json',
        //            rootProperty: 'etts',
        //            totalProperty: 'rc'
        //        }
        //    }
        //},
    }
});