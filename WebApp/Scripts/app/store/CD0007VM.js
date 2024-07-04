Ext.define('WEBAPP.store.CD0007VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.CD0007PICK',
        'WEBAPP.model.CD0007WH_NO'
    ],
    stores: {
        CD0007PICK: {
            model: 'WEBAPP.model.CD0007PICK',
            pageSize: 20,
            remoteSort: true,
            sorters: [{ property: 'DOCNO_CNT', direction: 'ASC' }],    // 人員代碼
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'    // by default GET
                },
                url: '/api/CD0007/QueryD',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        CD0007WH_NO: {
            model: 'WEBAPP.model.CD0007WH_NO',
            pageSize: 8,
            remoteSort: true,
            sorters: [{ property: 'WH_NO', direction: 'ASC' }],    // 人員代碼
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'    // by default GET
                },
                url: '/api/CD0007/GetWH_No',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }
       
    }
});
