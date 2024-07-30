Ext.define('WEBAPP.store.BH.BH0002VM', {
    extend: 'Ext.app.ViewModel',
    //alias: 'viewmodel.AA0015',
    requires: [
        'WEBAPP.model.WB_MM_PO_M',
        'WEBAPP.model.WB_REPLY'
    ],
    stores: {
        PO: {
            model: 'WEBAPP.model.WB_MM_PO_M',
            pageSize: 500, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'PO_NO', direction: 'DESC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/BH0002/GetPoCombo',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    //totalProperty: 'rc'
                }
            }
        },
        PO_TSGH: {
            model: 'WEBAPP.model.WB_MM_PO_M',
            pageSize: 500, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'PO_NO', direction: 'DESC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/BH0002/GetPoCombo_TSGH',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                }
            }
        },
        MMCODE: {
            model: 'WEBAPP.model.WB_REPLY',
            pageSize: 500, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/BH0002/GetMmcodeCombo',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    //totalProperty: 'rc'
                }
            }
        },
        WB_MM_PO_M: {
            model: 'WEBAPP.model.WB_MM_PO_M',
            pageSize: 500, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'mmcode', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/BH0002/GetPoMaster',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        WB_MM_PO_M_TSGH: {
            model: 'WEBAPP.model.WB_MM_PO_M',
            pageSize: 500, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'mmcode', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/BH0002/GetPoMaster_TSGH',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
        WB_REPLY: {
            model: 'WEBAPP.model.WB_REPLY',
            pageSize: 500, // 每頁顯示筆數
            remoteSort: true,
            sorters: [{ property: 'DNO, mmcode', direction: 'ASC' }], // 預設排序欄位
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/BH0002/GetPoDetail',
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        },
    }

});