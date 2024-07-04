Ext.define('WEBAPP.store.AA.AA0121VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.MI_WHMAST',
        'WEBAPP.model.UR_ID',
        'WEBAPP.model.MI_WHID'
    ],
    stores: {
        MI_WHID: {
            model: 'WEBAPP.model.MI_WHID',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'WH_NO', direction: 'ASC' }],   // 預設庫房代碼排序欄位 
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AA0121/QueryM',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }, MI_WHIDa: {
            model: 'WEBAPP.model.MI_WHID',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'WH_NO', direction: 'ASC' }],   // 預設庫房代碼排序欄位 
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/AA0121/QueryM',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        UR_ID: {
            model: 'WEBAPP.model.UR_ID',
            pageSize: 20,
            remoteSort: true,
            sorters: [{ property: 'TUSER', direction: 'ASC' }],    // 人員代碼
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'    // by default GET
                },
                url: '/api/AA0121/QueryD',
                //np: { },
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        MI_WHMAST: {
            model: 'WEBAPP.model.MI_WHMAST',
            pageSize: 20,
            remoteSort: true,
            sorters: [{ property: 'WH_NO', direction: 'ASC' }],    // 庫房代碼
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'       // by default GET
                },
                url: '/api/AA0121/QWHMASTAll',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
            ,
            listeners: {
                load: function (store, records, successful, eOpts) {
                      if (successful) {
                          //store.insert(0, { JOB_DES: '' });           //最前面增加一列空白選項(全部)
                          //alert("dd:");
                          //Ext.getCmp('BtnSel').setDisabled(true);
                      }
                }
            }

        }
    }
});