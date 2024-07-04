Ext.define('WEBAPP.store.CE0005VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.CHK_MAST',
        'WEBAPP.model.CHK_DETAIL',
        'WEBAPP.model.BC_WHCHKID'
    ],
    stores: {
        MasterAll: {
            model: 'WEBAPP.model.CHK_MAST',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'CHK_NO', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0005/MasterAll',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        DetailExclude: {
            model: 'WEBAPP.model.CHK_DETAIL',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0005/GetDetailExclude',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            },
        },
        DetailInclude: {
            model: 'WEBAPP.model.CHK_DETAIL',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0002/GetDetailInclude',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        MasterInsertList: {
            model: 'WEBAPP.model.CHK_MAST',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'CHK_NO', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0005/GetMasterInsertList',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        PickUsers: {
            model: 'WEBAPP.model.BC_WHCHKID',
            pageSize: 99999999, // 每頁顯示筆數
            //remoteSort: true,
            sorters: [{ property: 'WH_CHKUID', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0002/GetPickUsers',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        AddItemList: {
            model: 'WEBAPP.model.CHK_DETAIL',
            pageSize: 20, // 每頁顯示筆數
            //remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0005/GetAddItemList',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        CurrentUids: {
            model: 'WEBAPP.model.BC_WHCHKID',
            pageSize: 99999999, // 每頁顯示筆數
            //remoteSort: true,
            sorters: [{ property: 'WH_CHKUID', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CE0002/CurrentUids',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
    }
});