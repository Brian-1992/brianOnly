Ext.define('WEBAPP.store.AA.AA0073VM', {
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
        AA0073: {
            //model: 'WEBAPP.model.MI_WLOCINV',
            //pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            //sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                timeout: 1200000,
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AA0073/All',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        YM: {
            model: 'WEBAPP.model.MI_MATCLASS',
            autoLoad: true,
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/AA0073/GetYmCombo',
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