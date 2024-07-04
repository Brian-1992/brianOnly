Ext.define('WEBAPP.store.CD0002VM', {
    extend: 'Ext.app.ViewModel',
    requires: [
        'WEBAPP.model.BC_WHPICKDOC',
        'WEBAPP.model.BC_WHPICK',
        'WEBAPP.model.ME_DOCM',
        'WEBAPP.model.ME_DOCD',
        'WEBAPP.model.BC_WHPICK_TEMP_LOTSUM',
        'WEBAPP.model.BC_WHPICKLOT',
    ],
    stores: {
        BC_WHPICKDOC: {
            model: 'WEBAPP.model.BC_WHPICKDOC',
            pageSize: 20,
            remoteSort: true,
            sorters: [{ property: 'INID', direction: 'ASC' }],
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST'
                },
                url: '/api/CD0002/MasterAll',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        BC_WHPICK: {
            model: 'WEBAPP.model.BC_WHPICK',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'DOCNO', direction: 'ASC' }, { property: 'SEQ', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CD0002/DetailAllByDocnos',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        ME_DOCM: {
            model: 'WEBAPP.model.ME_DOCM',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'DOCNO', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CD0002/GetMeDocms',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        ME_DOCD: {
            model: 'WEBAPP.model.ME_DOCD',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'SEQ', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CD0002/GetMeDocds',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        ComplexityMaster: {
            model: 'WEBAPP.model.BC_WHPICKDOC',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'DOCNO', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CD0002/GetComplexMaster',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        ComplexityDetail: {
            model: 'WEBAPP.model.BC_WHPICK',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'SEQ', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CD0002/GetComplexDetail',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        DistributeRegular: {
            model: 'WEBAPP.model.BC_WHPICKDOC',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'COMPLEXITY', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CD0002/DistributeRegularByInid',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        DistributedMaster: {
            model: 'WEBAPP.model.BC_WHPICKLOT',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'LOT_NO', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CD0002/DistributedMaster',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        DistributedDetail: {
            model: 'WEBAPP.model.BC_WHPICKLOT',
            pageSize: 20, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'APPDEPT', direction: 'ASC' },{ property: 'DOCNO', direction: 'ASC' }, { property: 'SEQ', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/CD0002/DistributedDetail',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
    }
});