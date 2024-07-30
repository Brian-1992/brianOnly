Ext.define('WEBAPP.store.AB.AB0038', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.AB0038'
    ],
    stores: {
        AB0038_1: {
            model: 'WEBAPP.model.AB0038',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'ORDERCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                //url: '/api/AB0038/GetColList',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        AB0038_2: {
            model: 'WEBAPP.model.AB0038',
            pageSize: 20,
            remoteSort: true,
            sorters: [{ property: 'ORDERCODE', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                //url: '/api/AB0038/GetDefaultList',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        AB0038_3: {
            model: 'WEBAPP.model.AB0038',
            pageSize: 20,
            remoteSort: true,
            sorters: [{ property: 'ORDERCODE', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                //url: '/api/AB0038/GetDefaultList',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        AB0038_4: {
            model: 'WEBAPP.model.AB0038',
            pageSize: 20,
            remoteSort: true,
            sorters: [{ property: 'ORDERCODE', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                //url: '/api/AB0038/GetDefaultList',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        AB0038_5: {
            model: 'WEBAPP.model.AB0038',
            pageSize: 20,
            remoteSort: true,
            sorters: [{ property: 'LOAD_MSG', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                //url: '/api/AB0038/GetDefaultList',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        AB0038_6: {
            model: 'WEBAPP.model.AB0038',
            pageSize: 20,
            remoteSort: true,
            sorters: [{ property: 'ORDERCODE', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                //url: '/api/AB0038/GetDefaultList',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        }
    }

});