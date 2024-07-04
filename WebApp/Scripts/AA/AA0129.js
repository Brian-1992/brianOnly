Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'WH_NAME', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'INV_QTY', type: 'float' },
            { name: 'ONWAY_QTY', type: 'float' },
            { name: 'SAFE_QTY', type: 'float' },
            { name: 'OPER_QTY', type: 'float' },
            { name: 'SHIP_QTY', type: 'float' },
            { name: 'HIGH_QTY', type: 'float' }
        ]
    });

    Ext.define('T21Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'STORE_LOC', type: 'string' },
            { name: 'INV_QTY', type: 'string' }
        ]
    });

    Ext.define('T22Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'EXP_DATE', type: 'string' },
            { name: 'LOT_NO', type: 'string' },
            { name: 'INV_QTY', type: 'float' }
        ]
    });

    Ext.define('T23Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'WH_NAME', type: 'string' },
            { name: 'WH_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'INV_QTY', type: 'float' },
            { name: 'ONWAY_QTY', type: 'float' },
            { name: 'APL_INQTY', type: 'float' },
            { name: 'APL_OUTQTY', type: 'float' },
            { name: 'TRN_INQTY', type: 'float' },
            { name: 'TRN_OUTQTY', type: 'float' },
            { name: 'ADJ_INQTY', type: 'float' },
            { name: 'ADJ_OUTQTY', type: 'float' },
            { name: 'BAK_INQTY', type: 'float' },
            { name: 'BAK_OUTQTY', type: 'float' },
            { name: 'REJ_OUTQTY', type: 'float' },
            { name: 'DIS_OUTQTY', type: 'float' },
            { name: 'EXG_INQTY', type: 'float' },
            { name: 'EXG_OUTQTY', type: 'float' },
            { name: 'MIL_INQTY', type: 'float' },
            { name: 'MIL_OUTQTY', type: 'float' },
            { name: 'INVENTORYQTY', type: 'float' },
            { name: 'TUNEAMOUNT', type: 'float' },
            { name: 'USE_QTY', type: 'float' },
            { name: 'PRE_QTY', type: 'float' }
        ]
    });
    var user_kind = '';
    var wh_no = '';
    var mm_code = '';
    var winAA0129;
    var ctdmdccodes = "'0','2','3','4'";

    function getUserKind() {
        Ext.Ajax.request({
            url: '/api/AA0129/GetUserkind',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                user_kind = data;
                if (user_kind != '1') {
                    ctdmdccodes = "'0','1','2','3','4'";
                    Ext.getCmp('ctdmdccodes').setValue(['0', '1', '2', '3', '4']);
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getUserKind();

    var mat_class_store = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AA0129/GetMAT_CLASSCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, listeners: {
            beforeload: function (store, options) {
                var wh_kind = '';
                var index = wh_store.find('VALUE', T1Query.getForm().findField('P0').getValue());
                if (index != -1)
                    wh_kind = wh_store.data.items[wh_store.find('VALUE', T1Query.getForm().findField('P0').getValue())].data.COMBITEM
                var np = {
                    p0: wh_kind, 
                    user_kind: user_kind
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store) {
                store.insert(0, { TEXT: '', VALUE: '' });
            }
        },
        autoLoad: true
    });

    var wh_store = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AA0129/GetWhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: user_kind
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store) {
                store.insert(0, { TEXT: '', VALUE: '', COMBITEM: '' });
            }
        },
        autoLoad: true
    });

    var QMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0129/GetMmcodeCombo',
        //extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Query.getForm().findField('P1').getValue()
            };
        }
    });

    var ctdmdccode_store = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'GET' // by default GET
            },
            url: '/api/AA0129/GetCtdmdccodeCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            xtype: 'combo',
            fieldLabel: '庫房',
            id: 'P0',
            queryMode: 'local',
            store: wh_store,
            displayField: 'TEXT',
            valueField: 'VALUE',
            matchFieldWidth: false,
            listConfig: {
                width: 230
            },
            listeners: {
                select: function (combo, record, index) {
                    if (user_kind=="S"){
                        Ext.getCmp('P1').setValue("");
                        Ext.getCmp('P1').setRawValue("");
                        mat_class_store.load();
                    }
                }
            }
        }, {
            id: 'P1',
            xtype: 'combo',
            fieldLabel: '物料分類',
            queryMode: 'local',
            store: mat_class_store,
            displayField: 'TEXT',
            valueField: 'VALUE'
        }, QMMCode,
        {
            xtype: 'textfield',
            id: 'P2',
            fieldLabel: '中文品名'
        }, {
            xtype: 'textfield',
            id: 'P3',
            fieldLabel: '英文品名'
        }, {
            xtype: 'combo',
            fieldLabel: '各庫停用碼',
            name: 'ctdmdccodes',
            id: 'ctdmdccodes',
            store: ctdmdccode_store,
            queryMode: 'local',
            multiSelect: true,
            displayField: 'TEXT',
            valueField: 'VALUE',
            value: ['0', '2', '3', '4'],
            listeners: {
                select: function (combo, record, index) {
                    ctdmdccodes = '';
                    for (var i = 0; i < record.length; i++) {
                        if (ctdmdccodes != '') {
                            ctdmdccodes += ',';
                        }
                        ctdmdccodes += ("'" + record[i].data.VALUE + "'" );
                    }
                }
            }
        },],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: T1Load
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
            }
        }]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'WH_NAME', direction: 'ASC' },{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0129/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                
                var np = {
                    user_kind: user_kind,
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('MMCODE').getValue(),
                    p3: T1Query.getForm().findField('P2').getValue(),
                    p4: T1Query.getForm().findField('P3').getValue(),
                    ctdmdccodes: ctdmdccodes
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    var T21Store = Ext.create('Ext.data.Store', {
        model: 'T21Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'STORE_LOC', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0129/AllD1',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: wh_no,
                    p1: mm_code,
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    var T22Store = Ext.create('Ext.data.Store', {
        model: 'T22Model',
        pageSize: 9999, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'EXP_DATE', direction: 'ASC' }, { property: 'LOT_NO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AA0129/AllD2',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: wh_no,
                    p1: mm_code,
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    var T23Store = Ext.create('Ext.data.Store', {
        model: 'T23Model',
        pageSize: 1, 
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AA0129/AllD3',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: wh_no,
                    p1: mm_code,
                };
                Ext.apply(store.proxy.extraParams, np);
            }, load: function (store) {
                if (store.data.length > 0) {
                    T23Form.loadRecord(store.getAt(0));
                    if (store.getAt(0).data.WEXP_ID == "Y") {
                        TATabs.child('#exp').tab.show();
                    } else TATabs.child('#exp').tab.hide();
                }
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
        viewport.down('#form').collapse();
    }
    showDetail = function (pWh, pMmcode) {
        wh_no = pWh;
        mm_code = pMmcode;
        T21Tool.moveFirst();
        T22Store.load();
        T23Store.load();
        var loc = TATabs.child('#loc');
        TATabs.setActiveTab(loc);
        showWin();
    }
    function showWin() {
        if (winAA0129.hidden) {
            winAA0129.show();
        }
    }
    function hideWin() {
        if (!winAA0129.hidden) {
            winAA0129.hide();
        }
    }
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }
        ],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "庫房",
            dataIndex: 'WH_NAME',
            width: 120
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80,
            renderer: function (val, meta, record) {
                var arr = record.data.WH_NAME.split(" "); 
                return '<a href=javascript:showDetail("' + arr[0]+ '","' + record.data.MMCODE + '"); >' + val +'</a>';
            }
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            flex: 0.5,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            flex: 0.5,
            sortable: true
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 50
        }, {
          //  xtype: 'numbercolumn',
            text: "庫存量",
            dataIndex: 'INV_QTY',
            style: 'text-align:left',
            align: 'right',
       //     format: '0,000.0000',
            width: 100
        }, {
       //     xtype: 'numbercolumn',
            text: "在途量",
            dataIndex: 'ONWAY_QTY',
            style: 'text-align:left',
            align: 'right',
            format: '0,000',
            width: 80
        }, {
        //    xtype: 'numbercolumn',
            text: "安全量",
            dataIndex: 'SAFE_QTY',
            style: 'text-align:left',
            align: 'right',
      //      format: '0,000',
            width: 80
        }, {
       //     xtype: 'numbercolumn',
            text: "作業量",
            dataIndex: 'OPER_QTY',
            style: 'text-align:left',
            align: 'right',
      //      format: '0,000',
            width: 80
        }, {
       //     xtype: 'numbercolumn',
            text: "運補量",
            dataIndex: 'SHIP_QTY',
            style: 'text-align:left',
            align: 'right',
       //     format: '0,000',
            width: 80
        }, {
       //     xtype: 'numbercolumn',
            text: "基準量",
            dataIndex: 'HIGH_QTY',
            style: 'text-align:left',
            align: 'right',
       //     format: '0,000',
            width: 80
        }
        ]
    });

    var T21Tool = Ext.create('Ext.PagingToolbar', {
        store: T21Store,
        displayInfo: true,
        border: false,
        plain: true
    }); 

    var T21Grid = Ext.create('Ext.grid.Panel', {
        store: T21Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T21Tool]
        }
        ],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            sortable: false,
            width: 100
        }, {
            text: "儲位代碼",
            dataIndex: 'STORE_LOC',
            width: 150
        }, {
            text: "數量",
            dataIndex: 'INV_QTY',
            align: 'right',
         //   format: '0,000.0000',
            style: 'text-align:left',
            width: 120
        }
        ]
    });

    var T22Grid = Ext.create('Ext.grid.Panel', {
        store: T22Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            sortable: false,
            width: 100
        }, {
            text: "效期",
            dataIndex: 'EXP_DATE',
            width: 100
        }, {
            text: "批號",
            dataIndex: 'LOT_NO',
            width: 140
        }, {
            text: "數量",
            dataIndex: 'INV_QTY',
            align: 'right',
        //    format: '0,000.0000',
            style: 'text-align:left',
            width: 100
        }
        ]
    });

    var T23Form = Ext.widget({
        xtype: 'form',
        layout: {
            type: 'table',
            columns:4
        },
        frame: false,
        autoScroll: true,
        collapsible: false,
        bodyPadding: '5 5 5',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 100,
            labelAlign: 'right'
        },
        defaultType: 'displayfield',
        items: [{
            fieldLabel: '倉庫',
            name: 'WH_NAME',
            colspan: 2
        }, {
            fieldLabel: '院內碼',
            name: 'MMCODE',
            colspan: 1
            },
            {
                fieldLabel: '各庫停用碼', name: 'CTDMDCCODE'

            }, {
            fieldLabel: '中文品名',
            name: 'MMNAME_C',
            colspan: 4
        }, {
            fieldLabel: '英文品名',
            name: 'MMNAME_E',
            colspan: 4
        }, {
            fieldLabel: '上期結存',
            name: 'PRE_QTY',
            colspan: 4//,
           // renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        }, {
            fieldLabel: '庫存數量', name: 'INV_QTY'//,
          //  renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        }, {
            fieldLabel: '在途數量', name: 'ONWAY_QTY'//,
           // renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '入庫總量', name: 'APL_INQTY'//,
        //    renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '撥發總量', name: 'APL_OUTQTY'//,
          //  renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '調撥入總量', name: 'TRN_INQTY'//,
         //   renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '調撥出總量', name: 'TRN_OUTQTY'//,
         //   renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '調帳入總量', name: 'ADJ_INQTY'//,
        //    renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '調帳出總量', name: 'ADJ_OUTQTY'//,
        //    renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '繳回入庫總量', name: 'BAK_INQTY'//,
         //   renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '繳回出庫總量', name: 'BAK_OUTQTY'//,
        //    renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '退貨總量', name: 'REJ_OUTQTY'//,
        //    renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '報廢總量', name: 'DIS_OUTQTY'//,
          //  renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '換貨入庫總量', name: 'EXG_INQTY'//,
          //  renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '換貨出庫總量', name: 'EXG_OUTQTY'//,
         //   renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '戰備換入', name: 'MIL_INQTY'//,
         //   renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '戰備換出', name: 'MIL_OUTQTY'//,
         //   renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '盤點差異量', name: 'INVENTORYQTY'//,
        //    renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '軍品調帳金額 ', name: 'TUNEAMOUNT'//,
       //     renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        },
        {
            fieldLabel: '耗用總量', name: 'USE_QTY'//,
        //    renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.0000"); }
        }]
    });
    var TATabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        enableTabScroll: true,
        layout: 'fit',
        defaults: {
            layout: 'fit',
            autoScroll: true,
            closabel: false
        },
        items: [
            { title: '儲位', itemId: 'loc', items: [T21Grid] },
            { title: '效期', itemId: 'exp', items: [T22Grid] },
            { title: '庫房庫存', items: [T23Form] }
        ]
    });
    
    if (!winAA0129) {
        winAA0129 = Ext.widget('window', {
            title: '',
            closeAction: 'hide',
            width: '80%',
            height: '80%',
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [
                TATabs
            ],
            buttons: [{
                text: '關閉',
                handler: function () {
                    hideWin();
                }
            }]
        });
    }
    
    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            width:'80%',
            title: '',
            border: false,
            items: [T1Grid]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: '20%',
            title: '',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Query]
        }
        ]
    });

    T1Query.getForm().findField('MMCODE').focus();
});
