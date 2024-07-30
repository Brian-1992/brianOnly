Ext.define('WEBAPP.store.FA0028VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.MI_WLOCINV',
        'WEBAPP.model.ME_DOCD',
        'WEBAPP.model.MI_WHMAST',
        'WEBAPP.model.MI_MATCLASS',
        'WEBAPP.model.UserInfo',
        'WEBAPP.model.ParamD',
        'WEBAPP.model.MI_MAST'
    ],
    stores: {
        MI_WLOCINV: {
            model: 'WEBAPP.model.MI_WLOCINV',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/FA0028/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        MAT_CLASS: {
            model: 'WEBAPP.model.MI_MATCLASS',
            //pageSize: 1000,
            autoLoad: true,
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/FA0028/GetMatClassCombo',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                    //totalProperty: 'rc'
                }
            }
        },
        WH_NO: {
            model: 'WEBAPP.model.MI_WHMAST',
            //pageSize: 1000,
            autoLoad: true,
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/FA0028/GetWhnoCombo',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                    //totalProperty: 'rc'
                }
            }
        },
        USER_INFO: {
            extend: 'Ext.data.Store',
            model: 'WEBAPP.model.UserInfo',
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/Acct/Info',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                }
            }
        }
    }

});