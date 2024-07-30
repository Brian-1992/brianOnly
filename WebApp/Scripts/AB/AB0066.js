Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var MMCODEComboGet = '../../../api/AB0066/GetMMCODECombo';
    var WH_NOComboGet = '../../../api/AB0066/WH_NOCombo';
    var WH_GRADEComboGet = '../../../api/AB0066/WH_GRADECombo';
    var E_RestrictCodeComboGet = '../../../api/AB0066/E_RestrictCodeCombo';

    var T1Name = "藥品庫存資料查詢";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var MMCODEComboGetStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: MMCODEComboGet,
            params: { limit: 10, page: 1, start: 0 },
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });


    var WH_NOComboGetStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: WH_NOComboGet,
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    var WH_GRADEComboGetStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: WH_GRADEComboGet,
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    var E_RestrictCodeComboGetStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: E_RestrictCodeComboGet,
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P2',
        name: 'P2',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        width: 300,
        labelWidth: 65,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0066/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 85;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,     // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'panel',
                id: 'PanelP1',
                bodyStyle: 'padding: 3px 5px;',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'monthfield',
                        id: 'P0',
                        name: 'P0',
                        fieldLabel: '查詢月份',
                        width: 170,
                        labelWidth: 90,
                        fieldCls: 'required',
                        allowBlank: false,
                        padding: '0 4 0 0'
                    },
                    {
                        xtype: 'monthfield',
                        id: 'P1',
                        name: 'P1',
                        fieldLabel: '至',
                        width: 90,
                        labelWidth: 12,
                        labelSeparator: '',
                        fieldCls: 'required',
                        allowBlank: false,
                        padding: '0 4 0 0'
                    },
                    T1QuryMMCode,
                    {
                        xtype: 'button',
                        text: '查詢',
                        style: 'margin:0px 5px 0px 20px;',
                        handler: T1Load,
                    },
                    {
                        xtype: 'button',
                        text: '清除',
                        style: 'margin:0px 5px;',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            msglabel('訊息區:');
                        }
                    }
                ]
            }
            ,
            {
                xtype: 'panel',
                id: 'PanelP2',
                bodyStyle: 'padding: 3px 5px;',
                border: false,
                layout: 'hbox',
                defaults: {
                    anchor: '100%',
                    forceSelection: true,
                    labelAlign: 'right',
                },
                items: [
                    {
                        xtype: 'combobox',
                        store: WH_NOComboGetStore,
                        width: 320,
                        labelWidth: 90,
                        id: 'P3',
                        name: 'P3',
                        //fieldCls: 'required',
                        //allowBlank: false,
                        fieldLabel: '庫別代碼',
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE'
                    },
                    {
                        xtype: 'combobox',
                        store: WH_GRADEComboGetStore,
                        width: 200,
                        labelWidth: 90,
                        id: 'P4',
                        name: 'P4',
                        fieldLabel: '庫存類別歸屬',
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        multiSelect: true
                    },
                    {
                        xtype: 'combobox',
                        store: E_RestrictCodeComboGetStore,
                        width: 200,
                        labelWidth: 65,
                        id: 'P5',
                        name: 'P5',
                        fieldLabel: '管制用藥',
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE'
                    },
                    {
                        xtype: 'checkboxfield',
                        width: 100,
                        labelWidth: 65,
                        id: 'P6',
                        name: 'P6',
                        fieldLabel: '已停用'
                    }
                ]
            }
        ]
    });

    Ext.define('AB0066_Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DATA_YM', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'WH_NAME_C', type: 'string' },
            { name: 'PMN_INVQTY', type: 'string' },
            { name: 'APL_INQTY', type: 'string' },
            { name: 'APL_OUTQTY', type: 'string' },
            { name: 'INVENTORYQTY', type: 'string' },
            { name: 'ADJ_QTY', type: 'string' },
            { name: 'INV_QTY_End', type: 'string' },
            { name: 'INV_QTY_Now', type: 'string' },
            { name: 'E_ORDERDCFLAG', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'AB0066_Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        timeout: 60000,
        sorters: [{ property: 'DATA_YM', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }, { property: 'WH_NAME_C', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0066/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                //已停用打勾='Y' 否則='N'
                var chb_p6 = T1Query.getForm().findField('P6');
                var tmp_p6 = chb_p6.getValue() ? 'Y' : 'N';

                // 載入前將查詢條件值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getRawValue(),
                    p1: T1Query.getForm().findField('P1').getRawValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: tmp_p6
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T1Load() {
        if ((T1Query.getForm().findField('P0').getValue() == "" || T1Query.getForm().findField('P0').getValue() == null) ||
            (T1Query.getForm().findField('P1').getValue() == "" || T1Query.getForm().findField('P1').getValue() == null)) {

            Ext.Msg.alert('訊息', '需填查詢月份才能查詢');
        } else {
            T1Tool.moveFirst();
        }
        msglabel('訊息區:');
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '匯出',
                id: 'excel',
                name: 'excel',
                handler: function () {
                    if ((T1Query.getForm().findField('P0').getValue() == "" || T1Query.getForm().findField('P0').getValue() == null) ||
                        (T1Query.getForm().findField('P1').getValue() == "" || T1Query.getForm().findField('P1').getValue() == null)) {

                        Ext.Msg.alert('訊息', '需填查詢月份才能查詢');
                        return;
                    }

                    var fn = '藥品月結庫存資料查詢'+'.xlsx';
                    var p = new Array();
                    //已停用打勾='Y' 否則='N'
                    var chb_p6 = T1Query.getForm().findField('P6');
                    var tmp_p6 = chb_p6.getValue() ? 'Y' : 'N';
                    p.push({ name: 'FN', value: fn }); //檔名
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue }); //SQL篩選條件
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue }); //SQL篩選條件
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() }); //SQL篩選條件
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() }); //SQL篩選條件
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() }); //SQL篩選條件
                    p.push({ name: 'p5', value: T1Query.getForm().findField('P5').getValue() }); //SQL篩選條件
                    p.push({ name: 'p6', value: tmp_p6 }); //SQL篩選條件

                    PostForm('/api/AB0066/Excel', p);
                }
            }]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "年月",
            dataIndex: 'DATA_YM',
            width: 60
        }, {
            text: "藥品代碼",
            dataIndex: 'MMCODE',
            width: 90
        }, {
            text: "藥品名稱",
            dataIndex: 'MMNAME_E',
            width: 400
        }, {
            text: "庫別",
            dataIndex: 'WH_NAME_C',
            width: 150
        }, {
            style: 'text-align:left',
            align: 'right',
            text: "上月結存",
            dataIndex: 'PMN_INVQTY',
            width: 70
        }, {
            style: 'text-align:left',
            align: 'right',
            text: "本期入帳",
            dataIndex: 'APL_INQTY',
            width: 70
        }, {
            style: 'text-align:left',
            align: 'right',
            text: "本期出帳",
            dataIndex: 'APL_OUTQTY',
            width: 70
        }, {
            style: 'text-align:left',
            align: 'right',
            text: "盤點差",
            dataIndex: 'INVENTORYQTY',
            width: 70
        }, {
            style: 'text-align:left',
            align: 'right',
            text: "調帳差",
            dataIndex: 'ADJ_QTY',
            width: 70
        }, {
            style: 'text-align:left',
            align: 'right',
            text: "結存量",
            dataIndex: 'INV_QTY_End',
            width: 70
        }, {
            style: 'text-align:left',
            align: 'right',
            text: "現存量",
            dataIndex: 'INV_QTY_Now',
            width: 70
        }, {
            text: "各庫停用碼",
            dataIndex: 'E_ORDERDCFLAG',
            width: 90
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
            }
        }
    });

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
            title: '',
            border: false,
            items: [T1Grid]
        }]
    });
});
